using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using ECommerce.Api.Features.Products.CreateProduct;
using ECommerce.Api.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Tests.Features.Products.CreateProduct;

public sealed class CreateProductEndpointTests
{
    [Fact]
    public async Task Post_WithValidRequest_ReturnsCreatedAndPersistsProduct()
    {
        await using TestApplication application = await TestApplication.CreateAsync(
            TestContext.Current.CancellationToken);
        CreateProductRequest request = new(
            Name: "  Wireless Mouse  ",
            Sku: "  mouse-001  ",
            Brand: "  Example Brand  ",
            Price: 49.99m,
            StockQuantity: 10);

        HttpResponseMessage response = await application.Client.PostAsJsonAsync(
            "/api/products",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using JsonDocument body = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken),
            cancellationToken: TestContext.Current.CancellationToken);
        int id = body.RootElement.GetProperty("id").GetInt32();

        Assert.True(id > 0);
        Assert.Equal($"/api/products/{id}", response.Headers.Location?.ToString());

        await application.ExecuteDbContextAsync(async db =>
        {
            Product product = await db.Products.SingleAsync(TestContext.Current.CancellationToken);

            Assert.Equal(id, product.Id);
            Assert.Equal("Wireless Mouse", product.Name);
            Assert.Equal("MOUSE-001", product.Sku);
            Assert.Equal("Example Brand", product.Brand);
        });
    }

    [Fact]
    public async Task Post_WithInvalidRequest_ReturnsValidationProblem()
    {
        await using TestApplication application = await TestApplication.CreateAsync(
            TestContext.Current.CancellationToken);
        CreateProductRequest request = new(
            Name: string.Empty,
            Sku: string.Empty,
            Brand: string.Empty,
            Price: 0,
            StockQuantity: -1);

        HttpResponseMessage response = await application.Client.PostAsJsonAsync(
            "/api/products",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using JsonDocument body = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(400, body.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Validation failed.", body.RootElement.GetProperty("title").GetString());

        string[] codes = body.RootElement
            .GetProperty("errors")
            .EnumerateArray()
            .Select(error => error.GetProperty("code").GetString()!)
            .ToArray();

        Assert.Contains("Products.Name.Required", codes);
        Assert.Contains("Products.Sku.Required", codes);
        Assert.Contains("Products.Brand.Required", codes);
        Assert.Contains("Products.Price.NotPositive", codes);
        Assert.Contains("Products.StockQuantity.Negative", codes);
    }

    [Fact]
    public async Task Post_WithExistingSku_ReturnsConflictProblem()
    {
        await using TestApplication application = await TestApplication.CreateAsync(
            TestContext.Current.CancellationToken);

        await application.ExecuteDbContextAsync(async db =>
        {
            db.Products.Add(Product.Create(
                name: "Existing Mouse",
                sku: "MOUSE-001",
                brand: "Example Brand",
                price: 39.99m,
                stockQuantity: 5));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        });

        CreateProductRequest request = new(
            Name: "Another Mouse",
            Sku: "  mouse-001  ",
            Brand: "Another Brand",
            Price: 59.99m,
            StockQuantity: 3);

        HttpResponseMessage response = await application.Client.PostAsJsonAsync(
            "/api/products",
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using JsonDocument body = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(409, body.RootElement.GetProperty("status").GetInt32());

        Assert.Equal("A conflict occurred.",
            body.RootElement.GetProperty("title").GetString());


        var expectedError = ProductErrors.SkuConflict(request.Sku.Trim());
        Assert.Equal(
            expectedError.Code,
            body.RootElement
                .GetProperty("errors")[0]
                .GetProperty("code")
                .GetString());
    }

}
