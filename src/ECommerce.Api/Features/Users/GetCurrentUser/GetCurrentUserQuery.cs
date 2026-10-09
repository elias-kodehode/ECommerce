using System.Security.Claims;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Features.Users.Common;

namespace ECommerce.Api.Features.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<Result<UserResponse>>;