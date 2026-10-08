namespace ECommerce.Api.Features.Users.LoginUser;

public sealed record LoginUserRequest(
    string Email,
    string Password,
    bool RememberMe = false
    );
