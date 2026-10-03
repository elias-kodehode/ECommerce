using ECommerce.Api.Domain;
using FluentValidation;

namespace ECommerce.Api.Features.Products.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
	public CreateProductCommandValidator()
	{
		RuleFor(command => command.Name)
			.NotEmpty()
			.WithErrorCode("Products.Name.Required")
			.WithMessage("A product name is required.")
			.MaximumLength(Product.MaxNameLength)
			.WithErrorCode("Products.Name.TooLong")
			.WithMessage($"A product name cannot exceed {Product.MaxNameLength} characters.");

		RuleFor(command => command.Sku)
			.NotEmpty()
			.WithErrorCode("Products.Sku.Required")
			.WithMessage("A product SKU is required.")
			.MaximumLength(Product.MaxSkuLength)
			.WithErrorCode("Products.Sku.TooLong")
			.WithMessage($"A product SKU cannot exceed {Product.MaxSkuLength} characters.");

		RuleFor(command => command.Brand)
			.NotEmpty()
			.WithErrorCode("Products.Brand.Required")
			.WithMessage("A product brand is required.")
			.MaximumLength(Product.MaxBrandLength)
			.WithErrorCode("Products.Brand.TooLong")
			.WithMessage($"A product brand cannot exceed {Product.MaxBrandLength} characters.");

		RuleFor(command => command.Price)
			.GreaterThan(0)
			.WithErrorCode("Products.Price.NotPositive")
			.WithMessage("A product price must be greater than zero.")
			.PrecisionScale(18, 2, ignoreTrailingZeros: true)
			.WithErrorCode("Products.Price.InvalidPrecision")
			.WithMessage(
				"A product price can have at most 16 whole-number digits and two decimal places.");

		RuleFor(command => command.StockQuantity)
			.GreaterThanOrEqualTo(0)
			.WithErrorCode("Products.StockQuantity.Negative")
			.WithMessage("A product stock quantity cannot be negative.");
	}
}
