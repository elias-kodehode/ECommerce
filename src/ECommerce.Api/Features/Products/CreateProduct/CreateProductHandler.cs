using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace ECommerce.Api.Features.Products.CreateProduct;

public sealed class CreateProductHandler(
	IValidator<CreateProductCommand> validator,
	IMemoryCache cache,
	AppDbContext db,
	ILogger<CreateProductHandler> logger) : ICommandHandler<CreateProductCommand, Result<int>>
{
	public async Task<Result<int>> HandleAsync(CreateProductCommand command, CancellationToken ct)
	{
		var validationResult = await validator.ValidateAsync(command, ct);

		if(!validationResult.IsValid)
		{
			Error[] errors = [.. validationResult.Errors.Select(failure => Error.Validation(failure.ErrorCode, failure.ErrorMessage))];

			return Result<int>.Failure(errors);
		}

		string normalizedSku = command.Sku.Trim().ToUpperInvariant();

		if(await db.Products.AnyAsync(product => product.Sku == normalizedSku, ct))
		{
			return Result<int>.Failure(ProductErrors.SkuConflict(normalizedSku));
		}

		Product product = Product.Create(
			name: command.Name,
			sku: command.Sku,
			brand: command.Brand,
			price: command.Price,
			stockQuantity: command.StockQuantity);

		db.Products.Add(product);

		try
		{
			await db.SaveChangesAsync(ct);
		}
		catch(DbUpdateException exception) when(IsUniqueSkuViolation(exception))
		{
            return Result<int>.Failure(ProductErrors.SkuConflict(normalizedSku));
        }

		cache.Set(ProductCacheKeys.ById(product.Id), new ProductResponse(
			product.Id,
			product.Name,
			product.Sku,
			product.Brand,
			product.Price,
			product.StockQuantity));

		logger.LogInformation("Created product with ID: {id}, and cache key {key}", product.Id, ProductCacheKeys.ById(product.Id));
		return Result<int>.Success(product.Id);
	}

	private static bool IsUniqueSkuViolation(DbUpdateException exception)
	{
		return exception.InnerException is PostgresException
		{
			SqlState: PostgresErrorCodes.UniqueViolation,
			ConstraintName: "IX_Products_Sku"
		};
	}
}
