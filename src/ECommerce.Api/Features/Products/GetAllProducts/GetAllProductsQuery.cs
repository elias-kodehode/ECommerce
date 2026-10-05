using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;

namespace ECommerce.Api.Features.Products.GetAllProducts;

public sealed record GetAllProductsQuery(
	int Page,
	int PageSize
	) : IQuery<Result<GetAllProductsResponse>>;