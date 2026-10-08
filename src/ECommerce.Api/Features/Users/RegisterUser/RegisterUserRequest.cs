namespace ECommerce.Api.Features.Users.RegisterUser;

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string ConfirmPassword
    );