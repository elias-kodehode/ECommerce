using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;

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
        });
    }
}

public sealed record UpdateProductRequest(
    string? Name,
    string? Brand,
    string? Sku,
    decimal? Price,
    int? QuantityStock
    );