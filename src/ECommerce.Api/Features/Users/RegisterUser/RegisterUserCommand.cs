using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Features.Users.Common;

namespace ECommerce.Api.Features.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string ConfirmPassword
    ) : ICommand<Result<UserResponse>>;
