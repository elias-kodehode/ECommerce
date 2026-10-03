using ECommerce.Api.Domain;
using ECommerce.Api.Features.Products.CreateProduct;

namespace ECommerce.Api.Tests.Features.Products.CreateProduct;

public sealed class CreateProductCommandValidatorTests
{
	private readonly CreateProductCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_IsValid()
	{
		CreateProductCommand command = new(
			Name: "Wireless Mouse",
			Sku: "MOUSE-001",
			Brand: "Example Brand",
			Price: 49.99m,
			StockQuantity: 10);

		var result = _validator.Validate(command);

		Assert.True(result.IsValid);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void Validate_WithInvalidCommand_ReturnsExpectedErrors()
	{
		CreateProductCommand command = new(
			Name: string.Empty,
			Sku: string.Empty,
			Brand: string.Empty,
			Price: 0,
			StockQuantity: -1);

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.Name.Required");
		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.Sku.Required");
		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.Brand.Required");
		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.Price.NotPositive");
		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.StockQuantity.Negative");
	}

	[Fact]
	public void Validate_WithValuesExceedingMaximumLengths_ReturnsExpectedErrors()
	{
		CreateProductCommand command = new(
			Name: new string('N', Product.MaxNameLength + 1),
			Sku: new string('S', Product.MaxSkuLength + 1),
			Brand: new string('B', Product.MaxBrandLength + 1),
			Price: 1,
			StockQuantity: 0);

		var result = _validator.Validate(command);

		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.Name.TooLong");
		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.Sku.TooLong");
		Assert.Contains(result.Errors, error => error.ErrorCode == "Products.Brand.TooLong");
	}

	[Fact]
	public void Validate_WithPriceHavingMoreThanTwoDecimalPlaces_ReturnsInvalidPrecisionError()
	{
		CreateProductCommand command = new(
			Name: "Wireless Mouse",
			Sku: "MOUSE-001",
			Brand: "Example Brand",
			Price: 49.999m,
			StockQuantity: 10);


		var result = _validator.Validate(command);

		Assert.False(result.IsValid);

		var error = Assert.Single(result.Errors);
		Assert.Equal("Products.Price.InvalidPrecision", error.ErrorCode);
	}
}
