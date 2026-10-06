using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using FluentValidation;

namespace ECommerce.Api.Features.Products.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
	public CreateProductCommandValidator()
	{
		RuleFor(command => command.Name)
			.NotEmpty()
			.WithErrorCode(ProductErrors.NameRequired.Code)
			.WithMessage(ProductErrors.NameRequired.Description)
			.MaximumLength(Product.MaxNameLength)
			.WithErrorCode(ProductErrors.NameTooLong.Code)
			.WithMessage(ProductErrors.NameTooLong.Description);

		RuleFor(command => command.Sku)
			.NotEmpty()
			.WithErrorCode(ProductErrors.SkuRequired.Code)
			.WithMessage(ProductErrors.SkuRequired.Description)
			.MaximumLength(Product.MaxSkuLength)
			.WithErrorCode(ProductErrors.SkuTooLong.Code)
			.WithMessage(ProductErrors.SkuTooLong.Description);

		RuleFor(command => command.Brand)
			.NotEmpty()
			.WithErrorCode(ProductErrors.BrandRequired.Code)
			.WithMessage(ProductErrors.BrandRequired.Description)
			.MaximumLength(Product.MaxBrandLength)
			.WithErrorCode(ProductErrors.BrandTooLong.Code)
			.WithMessage(ProductErrors.BrandTooLong.Description);

		RuleFor(command => command.Price)
			.GreaterThan(0)
			.WithErrorCode(ProductErrors.PriceNotPositive.Code)
			.WithMessage(ProductErrors.PriceNotPositive.Description)
			.PrecisionScale(18, 2, ignoreTrailingZeros: true)
			.WithErrorCode(ProductErrors.PriceInvalidPrecision.Code)
			.WithMessage(ProductErrors.PriceInvalidPrecision.Description);

		RuleFor(command => command.StockQuantity)
			.GreaterThanOrEqualTo(0)
			.WithErrorCode(ProductErrors.StockQuantityNegative.Code)
			.WithMessage(ProductErrors.StockQuantityNegative.Description);
	}
}
