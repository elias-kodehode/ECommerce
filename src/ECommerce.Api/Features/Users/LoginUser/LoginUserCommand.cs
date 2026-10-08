using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;

namespace ECommerce.Api.Features.Users.LoginUser;

public sealed record LoginUserCommand(
    string Email,
    string Password,
    bool RememberMe
    ) : ICommand<Result>;