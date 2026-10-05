using System.Net;
using System.Text.Json;

namespace ECommerce.Api.Tests.Common;

public sealed class OpenApiDocumentationTests
{
	[Fact]
	public async Task OpenApiDocument_ContainsDocumentedProductOperations()
	{
		await using TestApplication application = await TestApplication.CreateAsync(
			TestContext.Current.CancellationToken);

		HttpResponseMessage response = await application.Client.GetAsync(
			"/openapi/v1.json",
			TestContext.Current.CancellationToken);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		using JsonDocument document = await JsonDocument.ParseAsync(
			await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken),
			cancellationToken: TestContext.Current.CancellationToken);

		JsonElement paths = document.RootElement.GetProperty("paths");
		JsonElement products = paths.GetProperty("/api/products");
		JsonElement createProduct = products.GetProperty("post");
		JsonElement getAllProducts = products.GetProperty("get");
		JsonElement getProductById = paths
			.GetProperty("/api/products/{id}")
			.GetProperty("get");

		Assert.Equal("CreateProduct", createProduct.GetProperty("operationId").GetString());
		Assert.True(createProduct.GetProperty("responses").TryGetProperty("201", out _));
		Assert.True(createProduct.GetProperty("responses").TryGetProperty("400", out _));
		Assert.True(createProduct.GetProperty("responses").TryGetProperty("409", out _));

		Assert.Equal("GetAllProducts", getAllProducts.GetProperty("operationId").GetString());
		Assert.True(getAllProducts.GetProperty("responses").TryGetProperty("200", out _));
		Assert.True(getAllProducts.GetProperty("responses").TryGetProperty("400", out _));

		Assert.Equal("GetProductById", getProductById.GetProperty("operationId").GetString());
		Assert.True(getProductById.GetProperty("responses").TryGetProperty("200", out _));
		Assert.True(getProductById.GetProperty("responses").TryGetProperty("404", out _));
	}
}
