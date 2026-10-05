using ECommerce.Api.Features.Products.Common;

namespace ECommerce.Api.Features.Products.GetAllProducts;

public sealed record GetAllProductsResponse(
	IReadOnlyCollection<ProductResponse> Products,
	int Page,
	int PageSize,
	int TotalCount,
	int TotalPages);
