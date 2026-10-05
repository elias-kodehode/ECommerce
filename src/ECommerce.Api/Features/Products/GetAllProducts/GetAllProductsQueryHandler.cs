using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Products.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Features.Products.GetAllProducts;

public sealed class GetAllProductsQueryHandler(
	IValidator<GetAllProductsQuery> validator,
	AppDbContext db) 
	: IQueryHandler<GetAllProductsQuery, Result<GetAllProductsResponse>>
{
	public async Task<Result<GetAllProductsResponse>> HandleAsync(GetAllProductsQuery query, CancellationToken ct)
	{

		var validationResult = await validator.ValidateAsync(query, ct);

		if(!validationResult.IsValid)
		{
			Error[] errors = [.. validationResult.Errors.Select(failure => Error.Validation(failure.ErrorCode, failure.ErrorMessage))];

			return Result<GetAllProductsResponse>.Failure(errors);
		}

		var totalCount = await db.Products.CountAsync(ct);
		int totalPages = (totalCount + query.PageSize - 1) / query.PageSize;

		var products = await db.Products
			.AsNoTracking()
			.OrderBy(product => product.Id)
			.Skip((query.Page - 1) * query.PageSize)
			.Take(query.PageSize)
			.Select(product => new ProductResponse(product.Id,
				product.Name,
				product.Sku,
				product.Brand,
				product.Price,
				product.StockQuantity))
			.ToListAsync(ct);

		var response = new GetAllProductsResponse(
			products,
			query.Page,
			query.PageSize,
			totalCount,
			totalPages
		);


		return Result<GetAllProductsResponse>.Success(response);
	}
}
