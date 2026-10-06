namespace ECommerce.Api.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    string? Name,
    string? Brand,
    string? Sku,
    decimal? Price,
    int? QuantityStock
    );
