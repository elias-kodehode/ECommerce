using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;

namespace ECommerce.Api.Features.Products.GetAllProducts;

public sealed class GetAllProductsEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/products", async (IQueryDispatcher dispatcher, CancellationToken ct, int page = 1, int pageSize = 20) =>
		{
			var query = new GetAllProductsQuery(page, pageSize);
			var result = await dispatcher.SendAsync(query, ct);


			return result.Match(
				onSuccess: products => Results.Ok(products),
				onFailure: errors => errors.ToProblemDetails());
		})
		.WithName("GetAllProducts")
		.WithSummary("Get products")
		.WithDescription("Returns a page of products ordered by product ID. Page size must be between 1 and 100.")
		.WithTags("Products")
		.Produces<GetAllProductsResponse>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest);
	}
}
