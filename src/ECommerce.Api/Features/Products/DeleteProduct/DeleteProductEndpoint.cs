using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Data;

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
                onSuccess: () => Results.Ok(),
                onFailure: errors => errors.ToProblemDetails()
            );
        });
    }
}
