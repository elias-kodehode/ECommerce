using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;

namespace ECommerce.Api.Features.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string Brand,
    decimal Price,
    int StockQuantity) : ICommand<Result<int>>;
