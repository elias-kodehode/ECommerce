using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.CreateProduct;
using ECommerce.Api.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ECommerce.Api.Tests.Features.Products.CreateProduct;

public sealed class CreateProductHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_PersistsProductAndReturnsId()
    {
        await using AppDbContext db = TestDbContextFactory.Create();
        CreateProductHandler handler = CreateHandler(db);
        CreateProductCommand command = new(
            Name: "  Wireless Mouse  ",
            Sku: "  mouse-001  ",
            Brand: "  Example Brand  ",
            Price: 49.99m,
            StockQuantity: 10);

        Result<int> result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);

        Product product = await db.Products.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(result.Value, product.Id);
        Assert.Equal("Wireless Mouse", product.Name);
        Assert.Equal("MOUSE-001", product.Sku);
        Assert.Equal("Example Brand", product.Brand);
        Assert.Equal(49.99m, product.Price);
        Assert.Equal(10, product.StockQuantity);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_ReturnsErrorsWithoutPersistingProduct()
    {
        await using AppDbContext db = TestDbContextFactory.Create();
        CreateProductHandler handler = CreateHandler(db);
        CreateProductCommand command = new(
            Name: string.Empty,
            Sku: string.Empty,
            Brand: string.Empty,
            Price: 0,
            StockQuantity: -1);

        Result<int> result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Products.Name.Required");
        Assert.Contains(result.Errors, error => error.Code == "Products.Sku.Required");
        Assert.Contains(result.Errors, error => error.Code == "Products.Brand.Required");
        Assert.Contains(result.Errors, error => error.Code == "Products.Price.NotPositive");
        Assert.Contains(result.Errors, error => error.Code == "Products.StockQuantity.Negative");
        Assert.Empty(await db.Products.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WithExistingSku_ReturnsConflictWithoutPersistingProduct()
    {
        await using AppDbContext db = TestDbContextFactory.Create();
        Product existingProduct = Product.Create(
            name: "Existing Mouse",
            sku: "MOUSE-001",
            brand: "Example Brand",
            price: 39.99m,
            stockQuantity: 5);

        db.Products.Add(existingProduct);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        CreateProductHandler handler = CreateHandler(db);
        CreateProductCommand command = new(
            Name: "Another Mouse",
            Sku: "  mouse-001  ",
            Brand: "Another Brand",
            Price: 59.99m,
            StockQuantity: 3);

        Result<int> result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Error error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("Products.Sku.Conflict", error.Code);
        Assert.Equal(1, await db.Products.CountAsync(TestContext.Current.CancellationToken));
    }

    private static CreateProductHandler CreateHandler(AppDbContext db)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        ILogger<CreateProductHandler> logger = NullLogger<CreateProductHandler>.Instance;
        return new(new CreateProductCommandValidator(),cache, db, logger);
    }

}
