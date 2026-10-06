using ECommerce.Api.Common.Results;
using ECommerce.Api.Domain;

namespace ECommerce.Api.Features.Products.Common;

public static class ProductErrors
{
    public static Error NotFound(int id) => Error.NotFound(
            "Product.NotFound",
            $"Product with id {id} was not found.");


    public static Error SkuConflict(string sku) =>
        Error.Conflict(
            "Product.Sku.Conflict",
            $"A product with SKU '{sku}' already exists.");

    public static readonly Error NameRequired = Error.Validation(
        "Products.Name.Required",
        "A product name is required.");

    public static readonly Error NameTooLong = Error.Validation(
        "Products.Name.TooLong",
        $"A product name cannot exceed {Product.MaxNameLength} characters.");

    public static readonly Error SkuRequired = Error.Validation(
        "Products.Sku.Required",
        "A product SKU is required.");

    public static readonly Error SkuTooLong = Error.Validation(
        "Products.Sku.TooLong",
        $"A product SKU cannot exceed {Product.MaxSkuLength} characters.");

    public static readonly Error BrandRequired = Error.Validation(
        "Products.Brand.Required",
        "A product brand is required.");

    public static readonly Error BrandTooLong = Error.Validation(
        "Products.Brand.TooLong",
        $"A product brand cannot exceed {Product.MaxBrandLength} characters.");

    public static readonly Error PriceNotPositive = Error.Validation(
        "Products.Price.NotPositive",
        "A product price must be greater than zero.");

    public static readonly Error PriceInvalidPrecision = Error.Validation(
        "Products.Price.InvalidPrecision",
        "A product price can have at most 16 whole-number digits and two decimal places.");

    public static readonly Error StockQuantityNegative = Error.Validation(
        "Products.StockQuantity.Negative",
        "A product stock quantity cannot be negative.");
}
