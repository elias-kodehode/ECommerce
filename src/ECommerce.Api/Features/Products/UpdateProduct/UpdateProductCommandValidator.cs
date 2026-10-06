using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using FluentValidation;

namespace ECommerce.Api.Features.Products.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithErrorCode(ProductErrors.NameRequired.Code)
                .WithMessage(ProductErrors.NameRequired.Description)
            .MaximumLength(Product.MaxNameLength)
                .WithErrorCode(ProductErrors.NameTooLong.Code)
                .WithMessage(ProductErrors.NameTooLong.Description)
            .When(x => x.Name is not null);

        RuleFor(x => x.Sku)
            .NotEmpty()
                .WithErrorCode(ProductErrors.SkuRequired.Code)
                .WithMessage(ProductErrors.SkuRequired.Description)
            .MaximumLength(Product.MaxSkuLength)
                .WithErrorCode(ProductErrors.SkuTooLong.Code)
                .WithMessage(ProductErrors.SkuTooLong.Description)
            .When(x => x.Sku is not null);

        RuleFor(x => x.Brand)
            .NotEmpty()
                .WithErrorCode(ProductErrors.BrandRequired.Code)
                .WithMessage(ProductErrors.BrandRequired.Description)
            .MaximumLength(Product.MaxBrandLength)
                .WithErrorCode(ProductErrors.BrandTooLong.Code)
                .WithMessage(ProductErrors.BrandTooLong.Description)
            .When(x => x.Brand is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0m)
                .WithErrorCode(ProductErrors.PriceNotPositive.Code)
                .WithMessage(ProductErrors.PriceNotPositive.Description)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .WithErrorCode(ProductErrors.PriceInvalidPrecision.Code)
                .WithMessage(ProductErrors.PriceInvalidPrecision.Description)
            .When(x => x.Price.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
                .WithErrorCode(ProductErrors.StockQuantityNegative.Code)
                .WithMessage(ProductErrors.StockQuantityNegative.Description)
            .When(x => x.StockQuantity.HasValue);
    }
}
