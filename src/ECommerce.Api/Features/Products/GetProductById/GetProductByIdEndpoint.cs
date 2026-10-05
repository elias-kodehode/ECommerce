using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Features.Products.Common;

namespace ECommerce.Api.Features.Products.GetProductById;

public sealed class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:int}", async (
            int Id,
            IQueryDispatcher dispatcher, 
            CancellationToken ct) => 
        {

            var result = await dispatcher.SendAsync(new GetProductByIdQuery(Id), ct);

            return result.Match(
                onSuccess: product => Results.Ok(product),
                onFailure: errors => errors.ToProblemDetails());
        })
        .WithName("GetProductById")
        .WithSummary("Get a product by ID")
        .WithDescription("Returns a single product when the supplied product ID exists.")
        .WithTags("Products")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
