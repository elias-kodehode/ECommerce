using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Api.Features.Products.GetProductById;

public class GetProductByIdHandler(
    AppDbContext db,
    IMemoryCache cache,
    ILogger<GetProductByIdHandler> logger): IQueryHandler<GetProductByIdQuery, Result<Product>>
{
    public async Task<Result<Product>> HandleAsync(GetProductByIdQuery query, CancellationToken ct)
    {
        if(cache.TryGetValue<Product>($"product-id:{query.Id}", out var result) && result is not null)
        {
            logger.LogInformation("hit cache");
            return Result<Product>.Success(result);
        }

        var product = await db.Products.FindAsync([query.Id], ct);

        if(product is not null)
        {
            cache.Set("product-id:" + product.Id, product);
            logger.LogInformation("product was not previously cached and has been cached now");
            return Result<Product>.Success(product);
        }

        return Result<Product>.Failure(Error.NotFound("Product.NotFound", $"Product with ID {query.Id} was not found."));
    }
}
