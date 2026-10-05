using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Products.Common;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Api.Features.Products.DeleteProduct;

public class DeleteProductHandler(
    AppDbContext db,
    IMemoryCache cache
    ) : ICommandHandler<DeleteProductCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([command.Id], ct);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.Id));
        }

        db.Products.Remove(product); 

        await db.SaveChangesAsync(ct);

        cache.Remove(ProductCacheKeys.ById(command.Id));

        return Result.Success();
    }
}
