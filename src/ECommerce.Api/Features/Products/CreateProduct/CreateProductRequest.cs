namespace ECommerce.Api.Features.Products.CreateProduct;

public sealed record CreateProductRequest(
	string Name,
	string Sku,
	string Brand,
	decimal Price,
	int StockQuantity);