using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Domain;

namespace ECommerce.Api.Features.Products.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IQuery<Result<Product>>;
