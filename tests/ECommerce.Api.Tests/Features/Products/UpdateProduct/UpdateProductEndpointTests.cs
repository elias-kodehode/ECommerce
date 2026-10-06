using System.Net;
using System.Net.Http.Json;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using ECommerce.Api.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Tests.Features.Products.UpdateProduct;

public sealed class UpdateProductEndpointTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Patch_WithPriceOnlyUpdate_PreservesOtherFieldsAndRefreshesCachedProduct(bool includeExplicitNulls)
    {
        await using TestApplication application = await TestApplication.CreateAsync(TestContext.Current.CancellationToken);
        Product product = await SeedProductAsync(application);
        ProductResponse original = ToResponse(product);
        ProductResponse? cached = await application.Client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(original, cached);
        Dictionary<string, object?> request = new() { ["price"] = 39.99m };
        if (includeExplicitNulls)
        {
            request["name"] = null;
            request["sku"] = null;
            request["brand"] = null;
            request["quantityStock"] = null;
        }

        using HttpResponseMessage response = await application.Client.PatchAsJsonAsync(
            $"/api/products/{product.Id}", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductResponse expected = original with { Price = 39.99m };
        ProductResponse? updated = await response.Content.ReadFromJsonAsync<ProductResponse>(TestContext.Current.CancellationToken);
        Assert.Equal(expected, updated);
        ProductResponse? fetched = await application.Client.GetFromJsonAsync<ProductResponse>(
            $"/api/products/{product.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(expected, fetched);
        await AssertPersistedAsync(application, expected);
    }

    [Fact]
    public async Task Patch_WithAllFields_ReturnsNormalizedProductAndAcceptsZeroStock()
    {
        await using TestApplication application = await TestApplication.CreateAsync(TestContext.Current.CancellationToken);
        Product product = await SeedProductAsync(application);

        using HttpResponseMessage response = await application.Client.PatchAsJsonAsync(
            $"/api/products/{product.Id}",
            new { name = "  Updated Mouse  ", sku = "  mouse-002  ", brand = "  New Brand  ", price = 39.99m, quantityStock = 0 },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductResponse expected = new(product.Id, "Updated Mouse", "MOUSE-002", "New Brand", 39.99m, 0);
        ProductResponse? updated = await response.Content.ReadFromJsonAsync<ProductResponse>(TestContext.Current.CancellationToken);
        Assert.Equal(expected, updated);
        await AssertPersistedAsync(application, expected);
    }

    [Fact]
    public async Task Patch_WithInvalidSuppliedName_ReturnsValidationProblemWithoutSavingOtherChanges()
    {
        await using TestApplication application = await TestApplication.CreateAsync(TestContext.Current.CancellationToken);
        Product product = await SeedProductAsync(application);
        ProductResponse original = ToResponse(product);
        DateTime updatedAtUtc = product.UpdatedAtUtc;

        using HttpResponseMessage response = await application.Client.PatchAsJsonAsync(
            $"/api/products/{product.Id}",
            new { name = new string('N', Product.MaxNameLength + 1), price = 39.99m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertErrorAsync(response, ProductErrors.NameTooLong);
        await AssertPersistedAsync(application, original);
        await application.ExecuteDbContextAsync(async db =>
        {
            Product persisted = await db.Products.SingleAsync(TestContext.Current.CancellationToken);
            Assert.Equal(updatedAtUtc, persisted.UpdatedAtUtc);
        });
    }

    [Fact]
    public async Task Patch_WithMissingProduct_ReturnsNotFoundProblem()
    {
        await using TestApplication application = await TestApplication.CreateAsync(TestContext.Current.CancellationToken);

        using HttpResponseMessage response = await application.Client.PatchAsJsonAsync(
            "/api/products/123", new { price = 39.99m }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertErrorAsync(response, ProductErrors.NotFound(123));
        await application.ExecuteDbContextAsync(async db =>
            Assert.Empty(await db.Products.ToListAsync(TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Patch_WithAnotherProductsSku_ReturnsConflictProblemWithoutChangingEitherProduct()
    {
        await using TestApplication application = await TestApplication.CreateAsync(TestContext.Current.CancellationToken);
        Product product = await SeedProductAsync(application);
        Product otherProduct = await SeedProductAsync(application, sku: "KEYBOARD-001");
        ProductResponse original = ToResponse(product);
        ProductResponse otherOriginal = ToResponse(otherProduct);

        using HttpResponseMessage response = await application.Client.PatchAsJsonAsync(
            $"/api/products/{product.Id}", new { sku = "  keyboard-001  ", price = 39.99m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertErrorAsync(response, ProductErrors.SkuConflict("KEYBOARD-001"));
        await AssertPersistedAsync(application, original);
        await AssertPersistedAsync(application, otherOriginal);
    }

    private static async Task<Product> SeedProductAsync(TestApplication application, string sku = "MOUSE-001")
    {
        Product product = Product.Create("Wireless Mouse", sku, "Example Brand", 49.99m, 10);
        await application.ExecuteDbContextAsync(async db =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        return product;
    }

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Name, product.Sku, product.Brand, product.Price, product.StockQuantity);

    private static async Task AssertPersistedAsync(TestApplication application, ProductResponse expected)
    {
        await application.ExecuteDbContextAsync(async db =>
        {
            Product persisted = await db.Products.AsNoTracking().SingleAsync(
                product => product.Id == expected.Id, TestContext.Current.CancellationToken);
            Assert.Equal(expected, ToResponse(persisted));
        });
    }

    private static async Task AssertErrorAsync(HttpResponseMessage response, ECommerce.Api.Common.Results.Error expected)
    {
        ErrorResponse? problem = await response.Content.ReadFromJsonAsync<ErrorResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        ErrorResponseItem error = Assert.Single(problem.Errors);
        Assert.Equal(expected.Code, error.Code);
        Assert.Equal(expected.Description, error.Description);
    }
}
