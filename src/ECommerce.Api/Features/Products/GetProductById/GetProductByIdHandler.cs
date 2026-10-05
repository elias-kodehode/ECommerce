using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Products.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Api.Features.Products.GetProductById;

public class GetProductByIdHandler(
    AppDbContext db,
    IMemoryCache cache,
    ILogger<GetProductByIdHandler> logger): IQueryHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> HandleAsync(GetProductByIdQuery query, CancellationToken ct)
    {
        if(cache.TryGetValue<ProductResponse>(ProductCacheKeys.ById(query.Id), out var result) && result is not null)
        {
            logger.LogInformation("hit cache");
            return Result<ProductResponse>.Success(result);
        }

        var product = await db.Products
            .AsNoTracking()
            .Where(product => product.Id == query.Id)
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.Brand,
                product.Price,
                product.StockQuantity))
            .SingleOrDefaultAsync(ct);

        if(product is not null)
        {
            cache.Set(ProductCacheKeys.ById(product.Id), product);
            logger.LogInformation("product was not previously cached and has been cached now");
            return Result<ProductResponse>.Success(product);
        }

        return Result<ProductResponse>.Failure(Error.NotFound("Product.NotFound", $"Product with ID {query.Id} was not found."));
    }
}
