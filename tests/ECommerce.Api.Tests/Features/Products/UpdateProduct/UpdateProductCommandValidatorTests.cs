using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.Common;
using ECommerce.Api.Features.Products.UpdateProduct;

namespace ECommerce.Api.Tests.Features.Products.UpdateProduct;

public sealed class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    public static TheoryData<string?, string?, string?, string, string> InvalidTextUpdates => new()
    {
        { "", null, null, ProductErrors.NameRequired.Code, ProductErrors.NameRequired.Description },
        { " ", null, null, ProductErrors.NameRequired.Code, ProductErrors.NameRequired.Description },
        { new string('N', Product.MaxNameLength + 1), null, null, ProductErrors.NameTooLong.Code, ProductErrors.NameTooLong.Description },
        { null, "", null, ProductErrors.SkuRequired.Code, ProductErrors.SkuRequired.Description },
        { null, " ", null, ProductErrors.SkuRequired.Code, ProductErrors.SkuRequired.Description },
        { null, new string('S', Product.MaxSkuLength + 1), null, ProductErrors.SkuTooLong.Code, ProductErrors.SkuTooLong.Description },
        { null, null, "", ProductErrors.BrandRequired.Code, ProductErrors.BrandRequired.Description },
        { null, null, " ", ProductErrors.BrandRequired.Code, ProductErrors.BrandRequired.Description },
        { null, null, new string('B', Product.MaxBrandLength + 1), ProductErrors.BrandTooLong.Code, ProductErrors.BrandTooLong.Description }
    };

    public static TheoryData<decimal, string, string> InvalidPriceUpdates => new()
    {
        { 0m, ProductErrors.PriceNotPositive.Code, ProductErrors.PriceNotPositive.Description },
        { -1m, ProductErrors.PriceNotPositive.Code, ProductErrors.PriceNotPositive.Description },
        { 49.999m, ProductErrors.PriceInvalidPrecision.Code, ProductErrors.PriceInvalidPrecision.Description },
        { 10000000000000000m, ProductErrors.PriceInvalidPrecision.Code, ProductErrors.PriceInvalidPrecision.Description }
    };

    [Fact]
    public void Validate_WithOnlyZeroStockSupplied_AllowsOmittedFields()
    {
        UpdateProductCommand command = new(
            Name: null, Brand: null, Sku: null, Price: null, StockQuantity: 0)
        { Id = 1 };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithValuesAtSupportedLimits_IsValid()
    {
        UpdateProductCommand command = new(
            Name: new string('N', Product.MaxNameLength),
            Brand: new string('B', Product.MaxBrandLength),
            Sku: new string('S', Product.MaxSkuLength),
            Price: 9999999999999999.99m,
            StockQuantity: int.MaxValue)
        { Id = 1 };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [MemberData(nameof(InvalidTextUpdates))]
    public void Validate_WithInvalidSuppliedText_ReturnsSharedCodeAndDescription(
        string? name, string? sku, string? brand, string expectedCode, string expectedDescription)
    {
        UpdateProductCommand command = new(name, brand, sku, Price: null, StockQuantity: null)
        { Id = 1 };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(expectedCode, error.ErrorCode);
        Assert.Equal(expectedDescription, error.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidPriceUpdates))]
    public void Validate_WithInvalidSuppliedPrice_ReturnsSharedCodeAndDescription(
        decimal price, string expectedCode, string expectedDescription)
    {
        UpdateProductCommand command = new(
            Name: null, Brand: null, Sku: null, Price: price, StockQuantity: null)
        { Id = 1 };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(expectedCode, error.ErrorCode);
        Assert.Equal(expectedDescription, error.ErrorMessage);
    }

    [Fact]
    public void Validate_WithNegativeStock_ReturnsSharedCodeAndDescription()
    {
        UpdateProductCommand command = new(
            Name: null, Brand: null, Sku: null, Price: null, StockQuantity: -1)
        { Id = 1 };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ProductErrors.StockQuantityNegative.Code, error.ErrorCode);
        Assert.Equal(ProductErrors.StockQuantityNegative.Description, error.ErrorMessage);
    }
}
