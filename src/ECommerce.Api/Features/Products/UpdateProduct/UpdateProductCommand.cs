using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Features.Products.Common;

namespace ECommerce.Api.Features.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    string? Name,
    string? Brand,
    string? Sku,
    decimal? Price,
    int? StockQuantity) : ICommand<Result<ProductResponse>>
{
    public required int Id { get; init; }
}