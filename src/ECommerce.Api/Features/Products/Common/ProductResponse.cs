namespace ECommerce.Api.Features.Products.Common;

public sealed record ProductResponse(
	int Id,
	string Name,
	string Sku,
	string Brand,
	decimal Price,
	int StockQuantity);
