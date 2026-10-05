using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Features.Products.Common;

namespace ECommerce.Api.Features.Products.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IQuery<Result<ProductResponse>>;
