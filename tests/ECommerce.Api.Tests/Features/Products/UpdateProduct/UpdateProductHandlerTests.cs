using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using ECommerce.Api.Features.Products.UpdateProduct;
using ECommerce.Api.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace ECommerce.Api.Tests.Features.Products.UpdateProduct;

public sealed class UpdateProductHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_PersistsNormalizedProductAndInvalidatesOnlyItsCache()
    {
        await using AppDbContext db = TestDbContextFactory.Create();
        using MemoryCache cache = new(new MemoryCacheOptions());
        Product product = await SeedProductAsync(db);
        DateTime createdAtUtc = product.CreatedAtUtc;
        ProductResponse original = ToResponse(product);
        cache.Set(ProductCacheKeys.ById(product.Id), original);
        cache.Set(ProductCacheKeys.ById(999), original with { Id = 999 });
        UpdateProductHandler handler = CreateHandler(db, cache);
        UpdateProductCommand command = CreateCommand(
            product.Id, name: "  Updated Mouse  ", sku: "  mouse-002  ",
            brand: "  New Brand  ", price: 39.99m, stockQuantity: 0);
        DateTime beforeUpdateUtc = DateTime.UtcNow;

        Result<ProductResponse> result = await handler.HandleAsync(
            command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        ProductResponse expected = new(product.Id, "Updated Mouse", "MOUSE-002", "New Brand", 39.99m, 0);
        Assert.Equal(expected, result.Value);
        Assert.False(cache.TryGetValue(ProductCacheKeys.ById(product.Id), out _));
        Assert.Equal(original with { Id = 999 }, cache.Get<ProductResponse>(ProductCacheKeys.ById(999)));

        db.ChangeTracker.Clear();
        Product persisted = await db.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(expected, ToResponse(persisted));
        Assert.Equal(createdAtUtc, persisted.CreatedAtUtc);
        Assert.InRange(persisted.UpdatedAtUtc, beforeUpdateUtc, DateTime.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_DoesNotChangeProductOrCache()
    {
        await using AppDbContext db = TestDbContextFactory.Create();
        using MemoryCache cache = new(new MemoryCacheOptions());
        Product product = await SeedProductAsync(db);
        ProductResponse original = ToResponse(product);
        DateTime updatedAtUtc = product.UpdatedAtUtc;
        cache.Set(ProductCacheKeys.ById(product.Id), original);
        UpdateProductHandler handler = CreateHandler(db, cache);

        Result<ProductResponse> result = await handler.HandleAsync(
            CreateCommand(product.Id, name: "Changed Name", price: -1m),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductErrors.PriceNotPositive, Assert.Single(result.Errors));
        Assert.Same(original, cache.Get<ProductResponse>(ProductCacheKeys.ById(product.Id)));
        db.ChangeTracker.Clear();
        Product persisted = await db.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(original, ToResponse(persisted));
        Assert.Equal(updatedAtUtc, persisted.UpdatedAtUtc);
    }

    [Fact]
    public async Task HandleAsync_WithAnotherProductsSku_ReturnsConflictWithoutChangingProductOrCache()
    {
        await using AppDbContext db = TestDbContextFactory.Create();
        using MemoryCache cache = new(new MemoryCacheOptions());
        Product product = await SeedProductAsync(db);
        await SeedProductAsync(db, sku: "KEYBOARD-001");
        ProductResponse original = ToResponse(product);
        DateTime updatedAtUtc = product.UpdatedAtUtc;
        cache.Set(ProductCacheKeys.ById(product.Id), original);
        UpdateProductHandler handler = CreateHandler(db, cache);

        Result<ProductResponse> result = await handler.HandleAsync(
            CreateCommand(product.Id, sku: "  keyboard-001  ", price: 39.99m),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductErrors.SkuConflict("KEYBOARD-001"), Assert.Single(result.Errors));
        Assert.Same(original, cache.Get<ProductResponse>(ProductCacheKeys.ById(product.Id)));
        db.ChangeTracker.Clear();
        Product persisted = await db.Products.SingleAsync(
            item => item.Id == product.Id, TestContext.Current.CancellationToken);
        Assert.Equal(original, ToResponse(persisted));
        Assert.Equal(updatedAtUtc, persisted.UpdatedAtUtc);
    }

    [Fact]
    public async Task HandleAsync_WithItsOwnNormalizedSku_AllowsOtherFieldsToChange()
    {
        await using AppDbContext db = TestDbContextFactory.Create();
        using MemoryCache cache = new(new MemoryCacheOptions());
        Product product = await SeedProductAsync(db);
        ProductResponse original = ToResponse(product);
        UpdateProductHandler handler = CreateHandler(db, cache);

        Result<ProductResponse> result = await handler.HandleAsync(
            CreateCommand(product.Id, sku: "  mouse-001  ", price: 39.99m),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(original with { Price = 39.99m }, result.Value);
        db.ChangeTracker.Clear();
        Product persisted = await db.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(result.Value, ToResponse(persisted));
    }

    [Fact]
    public async Task HandleAsync_WhenSaveRejectsSkuAfterAvailabilityCheck_ReturnsConflictAndPreservesCache()
    {
        await using SaveFailureDbContext db = CreateSaveFailureContext();
        using MemoryCache cache = new(new MemoryCacheOptions());
        Product product = await SeedProductAsync(db);
        ProductResponse original = ToResponse(product);
        cache.Set(ProductCacheKeys.ById(product.Id), original);
        // Simulate a uniqueness failure at save time after the availability check has passed.
        db.SaveException = CreateDatabaseException(PostgresErrorCodes.UniqueViolation, "IX_Products_Sku");
        UpdateProductHandler handler = CreateHandler(db, cache);

        Result<ProductResponse> result = await handler.HandleAsync(
            CreateCommand(product.Id, sku: "  new-sku  ", price: 39.99m),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductErrors.SkuConflict("NEW-SKU"), Assert.Single(result.Errors));
        Assert.Same(original, cache.Get<ProductResponse>(ProductCacheKeys.ById(product.Id)));
        db.ChangeTracker.Clear();
        Product persisted = await db.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(original, ToResponse(persisted));
    }

    [Theory]
    [InlineData(PostgresErrorCodes.UniqueViolation, "IX_Products_Other")]
    [InlineData(PostgresErrorCodes.CheckViolation, "IX_Products_Sku")]
    public async Task HandleAsync_WithUnrelatedDatabaseError_PropagatesExceptionAndPreservesCache(
        string sqlState, string constraintName)
    {
        await using SaveFailureDbContext db = CreateSaveFailureContext();
        using MemoryCache cache = new(new MemoryCacheOptions());
        Product product = await SeedProductAsync(db);
        ProductResponse original = ToResponse(product);
        cache.Set(ProductCacheKeys.ById(product.Id), original);
        DbUpdateException expectedException = CreateDatabaseException(sqlState, constraintName);
        db.SaveException = expectedException;
        UpdateProductHandler handler = CreateHandler(db, cache);

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            handler.HandleAsync(CreateCommand(product.Id, sku: "NEW-SKU"), TestContext.Current.CancellationToken));

        Assert.Same(expectedException, exception);
        Assert.Same(original, cache.Get<ProductResponse>(ProductCacheKeys.ById(product.Id)));
        db.ChangeTracker.Clear();
        Product persisted = await db.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(original, ToResponse(persisted));
    }

    private static UpdateProductHandler CreateHandler(AppDbContext db, IMemoryCache cache) =>
        new(db, new UpdateProductCommandValidator(), cache);

    private static UpdateProductCommand CreateCommand(
        int id, string? name = null, string? sku = null, string? brand = null,
        decimal? price = null, int? stockQuantity = null) =>
        new(Name: name, Brand: brand, Sku: sku, Price: price, StockQuantity: stockQuantity) { Id = id };

    private static async Task<Product> SeedProductAsync(AppDbContext db, string sku = "MOUSE-001")
    {
        Product product = Product.Create("Wireless Mouse", sku, "Example Brand", 49.99m, 10);
        db.Products.Add(product);
        DateTime yesterdayUtc = DateTime.UtcNow.AddDays(-1);
        db.Entry(product).Property(item => item.CreatedAtUtc).CurrentValue = yesterdayUtc;
        db.Entry(product).Property(item => item.UpdatedAtUtc).CurrentValue = yesterdayUtc;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return product;
    }

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Name, product.Sku, product.Brand, product.Price, product.StockQuantity);

    private static SaveFailureDbContext CreateSaveFailureContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"update-save-failure-{Guid.NewGuid()}").Options);

    private static DbUpdateException CreateDatabaseException(string sqlState, string constraintName) =>
        new("Database rejected the update.", new PostgresException(
            messageText: "Constraint violation", severity: "ERROR", invariantSeverity: "ERROR",
            sqlState: sqlState, constraintName: constraintName));

    private sealed class SaveFailureDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        public DbUpdateException? SaveException { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            SaveException is null
                ? base.SaveChangesAsync(cancellationToken)
                : Task.FromException<int>(SaveException);
    }
}
