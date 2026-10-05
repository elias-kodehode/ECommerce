using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using ECommerce.Api.Features.Products.GetAllProducts;
using ECommerce.Api.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Tests.Features.Products.GetAllProducts;

public sealed class GetAllProductsQueryHandlerTests
{
	[Fact]
	public async Task HandleAsync_WithEmptyDatabase_ReturnsEmptyFirstPage()
	{
		await using AppDbContext db = TestDbContextFactory.Create();
		GetAllProductsQueryHandler handler = CreateHandler(db);
		GetAllProductsQuery query = new(Page: 1, PageSize: 20);

		Result<GetAllProductsResponse> result = await handler.HandleAsync(
			query,
			TestContext.Current.CancellationToken);

		Assert.True(result.IsSuccess);
		Assert.Empty(result.Value.Products);
		Assert.Equal(1, result.Value.Page);
		Assert.Equal(20, result.Value.PageSize);
		Assert.Equal(0, result.Value.TotalCount);
		Assert.Equal(0, result.Value.TotalPages);
	}

	[Fact]
	public async Task HandleAsync_WithSecondPage_ReturnsRemainingProductAndPaginationMetadata()
	{
		await using AppDbContext db = TestDbContextFactory.Create();
		db.Products.AddRange(
			CreateProduct("Keyboard", "KEYBOARD-001"),
			CreateProduct("Mouse", "MOUSE-001"),
			CreateProduct("Monitor", "MONITOR-001"));
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		GetAllProductsQueryHandler handler = CreateHandler(db);
		GetAllProductsQuery query = new(Page: 2, PageSize: 2);

		Result<GetAllProductsResponse> result = await handler.HandleAsync(
			query,
			TestContext.Current.CancellationToken);

		Assert.True(result.IsSuccess);
		ProductResponse product = Assert.Single(result.Value.Products);
		Assert.Equal("Monitor", product.Name);
		Assert.Equal(2, result.Value.Page);
		Assert.Equal(2, result.Value.PageSize);
		Assert.Equal(3, result.Value.TotalCount);
		Assert.Equal(2, result.Value.TotalPages);
	}

	[Fact]
	public async Task HandleAsync_WithInvalidPagination_ReturnsValidationErrors()
	{
		await using AppDbContext db = TestDbContextFactory.Create();
		GetAllProductsQueryHandler handler = CreateHandler(db);
		GetAllProductsQuery query = new(Page: 0, PageSize: 101);

		Result<GetAllProductsResponse> result = await handler.HandleAsync(
			query,
			TestContext.Current.CancellationToken);

		Assert.True(result.IsFailure);
		Assert.Contains(result.Errors, error => error.Code == "Products.Page.Invalid");
		Assert.Contains(result.Errors, error => error.Code == "Products.PageSize.Invalid");
	}

	private static GetAllProductsQueryHandler CreateHandler(AppDbContext db) =>
		new(new GetAllProductsQueryValidator(), db);

	private static Product CreateProduct(string name, string sku) =>
		Product.Create(
			name: name,
			sku: sku,
			brand: "Example Brand",
			price: 49.99m,
			stockQuantity: 10);

}
