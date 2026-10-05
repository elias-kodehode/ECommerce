using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;

namespace ECommerce.Api.Features.Products.GetProductById;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", async (
            int Id,
            IQueryDispatcher dispatcher, 
            CancellationToken ct) => 
        {
            GetProductByIdQuery query = new(
                Id
            );

            var result = await dispatcher.SendAsync(query,ct);

            return result.Match(
                onSuccess: product => Results.Ok(product),
                onFailure: errors => errors.ToProblemDetails());
        });
    }
}
