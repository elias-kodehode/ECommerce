using ECommerce.Api.Common.Results;

namespace ECommerce.Api.Features.Users.Common;

public static class UserErrors
{
    public static Error NotFound(int id) => Error.NotFound(
        "User.NotFound",
        $"User with id {id} was not found.");

    public static readonly Error EmailRequired = Error.Validation(
        "User.Email.Required",
        "An email address is required.");

    public static readonly Error EmailInvalid = Error.Validation(
        "User.Email.Invalid",
        "A valid email address is required.");

    public static readonly Error PasswordRequired = Error.Validation(
        "User.Password.Required",
        "A password is required.");

    public static readonly Error ConfirmPasswordRequired = Error.Validation(
        "User.ConfirmPassword.Required",
        "Password confirmation is required.");

    public static readonly Error PasswordsDoNotMatch = Error.Validation(
        "User.ConfirmPassword.Mismatch",
        "Passwords do not match.");

    public static readonly Error NotAllowed = Error.Forbidden(
        "User.Email.NotConfirmed",
        "Please confirm your email before logging in.");

    public static readonly Error LockedOut = Error.Forbidden(
        "User.Login.LockedOut",
        "Account locked out. Try again later.");
}
