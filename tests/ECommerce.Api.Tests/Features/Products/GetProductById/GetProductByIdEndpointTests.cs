using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Api.Features.Products.CreateProduct;
using ECommerce.Api.Tests.Common;

namespace ECommerce.Api.Tests.Features.Products.GetProductById;

public class GetProductByIdEndpointTests
{

    [Fact]
    public async Task Get_NonExistingProduct_Returns_404()
    {
        await using TestApplication application = await TestApplication.CreateAsync(
            TestContext.Current.CancellationToken);

        HttpResponseMessage response = await application.Client.GetAsync(
            "/api/products/1",
            cancellationToken: TestContext.Current.CancellationToken
        );
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Contains(result.Errors, error => error.Code == "Product.NotFound");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task Get_Existing_Product_Returns_Found()
    {
        await using TestApplication application = await TestApplication.CreateAsync(
            TestContext.Current.CancellationToken);

        CreateProductRequest request = new(
            Name: "Wireless Mouse",
            Sku: "mouse-001",
            Brand: "Example Brand",
            Price: 49.99m,
            StockQuantity: 10);

        var createResponse = await application.Client.PostAsJsonAsync(
            "/api/products",
            request,
            TestContext.Current.CancellationToken);


        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using JsonDocument body = await JsonDocument.ParseAsync(
        await createResponse.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken),
        cancellationToken: TestContext.Current.CancellationToken);

        int id = body.RootElement.GetProperty("id").GetInt32();


        var response = await application.Client.GetAsync(
            $"/api/products/{id}",
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(response);

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(product);
        Assert.Equal(request.Name, product.Name);
        Assert.Equal(request.Sku.ToUpper(), product.Sku.ToUpper());
        Assert.Equal(request.Brand, product.Brand);
        Assert.Equal(request.Price, product.Price);
        Assert.Equal(request.StockQuantity, product.StockQuantity);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public record ProductResponse(
        string Name,
        string Sku,
        string Brand,
        decimal Price,
        int StockQuantity
        );
}
