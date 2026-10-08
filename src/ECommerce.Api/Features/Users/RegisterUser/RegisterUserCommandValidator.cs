using ECommerce.Api.Features.Users.Common;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Features.Users.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{

    public RegisterUserCommandValidator()
    {

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithErrorCode(UserErrors.EmailRequired.Code)
                .WithMessage(UserErrors.EmailRequired.Description)
            .EmailAddress()
                .WithErrorCode(UserErrors.EmailInvalid.Code)
                .WithMessage(UserErrors.EmailInvalid.Description);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(UserErrors.PasswordRequired.Code)
            .WithMessage(UserErrors.PasswordRequired.Description);

        RuleFor(x => x.ConfirmPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithErrorCode(UserErrors.ConfirmPasswordRequired.Code)
                .WithMessage(UserErrors.ConfirmPasswordRequired.Description)
            .Equal(x => x.Password)
                .WithErrorCode(UserErrors.PasswordsDoNotMatch.Code)
                .WithMessage(UserErrors.PasswordsDoNotMatch.Description);
    }
}
