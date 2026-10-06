using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;

namespace ECommerce.Api.Features.Products.DeleteProduct;

public class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", async (
            int id,
            ICommandDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await dispatcher.SendAsync(command, ct);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: errors => errors.ToProblemDetails()
            );
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .WithDescription("Delete a product by Id")
        .WithTags("Products")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
