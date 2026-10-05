using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;

namespace ECommerce.Api.Features.Products.CreateProduct;

public sealed class CreateProductEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/products", async (CreateProductRequest request, ICommandDispatcher dispatcher, CancellationToken ct) =>
		{
			var command = new CreateProductCommand(
				request.Name,
				request.Sku,
				request.Brand,
				request.Price,
				request.StockQuantity
			);


			var result = await dispatcher.SendAsync(command, ct);


			return result.Match(
				onSuccess: id => Results.Created(
					$"/api/products/{id}",
					new CreateProductResponse(id)),
				onFailure: errors => errors.ToProblemDetails());
		})
		.WithName("CreateProduct")
		.WithSummary("Create a product")
		.WithDescription("Creates a new electronics product. Product SKUs must be unique.")
		.WithTags("Products")
		.Produces<CreateProductResponse>(StatusCodes.Status201Created)
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.ProducesProblem(StatusCodes.Status409Conflict);
	}
}
