using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;

namespace ECommerce.Api.Features.Products.DeleteProduct;

public sealed record DeleteProductCommand(
    int Id
    ) : ICommand<Result>;
