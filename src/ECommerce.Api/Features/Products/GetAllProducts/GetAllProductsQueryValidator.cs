using FluentValidation;

namespace ECommerce.Api.Features.Products.GetAllProducts;

public sealed class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
{
	//TODO: Use strongly typed error messages
	public GetAllProductsQueryValidator()
	{
		RuleFor(query => query.Page)
			.GreaterThanOrEqualTo(1)
			.WithErrorCode("Products.Page.Invalid")
			.WithMessage("Page must be at least 1.");

		RuleFor(query => query.PageSize)
			.InclusiveBetween(1, 100)
			.WithErrorCode("Products.PageSize.Invalid")
			.WithMessage("Page size must be between 1 and 100.");
	}
}
