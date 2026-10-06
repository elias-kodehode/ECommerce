using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Features.Products.Common;

namespace ECommerce.Api.Features.Products.UpdateProduct;

public sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{id}", async (int id, UpdateProductRequest request, ICommandDispatcher dispatcher, CancellationToken ct) => {

            var command = new UpdateProductCommand(
                Name: request.Name,
                Brand: request.Brand,
                Sku: request.Sku,
                Price: request.Price,
                StockQuantity: request.QuantityStock
                )
            { Id = id };

            var result = await dispatcher.SendAsync(command, ct);


            return result.Match(
                onSuccess: product => Results.Ok(product),
                onFailure: errors => errors.ToProblemDetails());
        })
        .WithName("UpdateProduct")
        .WithSummary("Update a product")
        .WithDescription("Partially updates an existing product by ID and returns the updated product. Omitted or null fields remain unchanged. Product SKUs must be unique.")
        .WithTags("Products")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
