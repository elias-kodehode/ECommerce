using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Products.CreateProduct;
using ECommerce.Api.Features.Products.GetProductById;
using ECommerce.Api.Tests.Common;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Tests.Features.Products.GetProductById;

public class GetProductByIdEndpointTests
{

    [Fact]
    public async Task Get_NonExistingProduct_Returns_404()
    {
        await using TestApplication application = await CreateApplicationAsync();

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
        await using TestApplication application = await CreateApplicationAsync();

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
    private static async Task<TestApplication> CreateApplicationAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        InMemoryDatabaseRoot databaseRoot = new();
        string databaseName = $"get-product-by-id-endpoint-tests-{Guid.NewGuid()}";
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(
                databaseName,
                databaseRoot));
        builder.Services.AddCqrs(typeof(GetProductByIdHandler).Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
        builder.Services.AddMemoryCache();
        WebApplication app = builder.Build();
        app.MapEndpoints();
        await app.StartAsync(TestContext.Current.CancellationToken);

        return new TestApplication(app, app.GetTestClient());
    }

    private sealed class TestApplication(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public IServiceProvider Services => app.Services;

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
