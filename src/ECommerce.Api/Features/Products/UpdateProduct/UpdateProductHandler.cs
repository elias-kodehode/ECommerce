using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Products.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace ECommerce.Api.Features.Products.UpdateProduct;

public sealed class UpdateProductHandler(
    AppDbContext db,
    IValidator<UpdateProductCommand> validator,
    IMemoryCache cache) : ICommandHandler<UpdateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> HandleAsync(UpdateProductCommand command, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command,ct);


        if (!validationResult.IsValid) {
            return Result<ProductResponse>.
                Failure(validationResult.Errors.Select(
                    failure => Error.Validation(failure.ErrorCode, failure.ErrorMessage)));
        }

        var product = await db.Products.FindAsync([command.Id], ct);

        if(product is null)
        {
            return Result<ProductResponse>.Failure(ProductErrors.NotFound(command.Id));
        }

        string normalizedSku = command.Sku?.Trim().ToUpperInvariant() ?? product.Sku;

        if(command.Sku is not null)
        {
            bool skuExists = await db.Products.AnyAsync(
                other => other.Sku == normalizedSku && other.Id != product.Id, ct);

            if (skuExists)
            {
                return Result<ProductResponse>.Failure(ProductErrors.SkuConflict(normalizedSku));
            }
        }

        product.UpdateDetails(
            name: command.Name ?? product.Name,
            sku: normalizedSku,
            brand: command.Brand ?? product.Brand,
            price: command.Price ?? product.Price,
            stockQuantity: command.StockQuantity ?? product.StockQuantity);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Products_Sku"
            })
        {
            return Result<ProductResponse>.Failure(
                ProductErrors.SkuConflict(normalizedSku));
        }

        cache.Remove(ProductCacheKeys.ById(product.Id));

        return Result<ProductResponse>.Success(new ProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Brand,
            product.Price,
            product.StockQuantity
            ));

    }
}
