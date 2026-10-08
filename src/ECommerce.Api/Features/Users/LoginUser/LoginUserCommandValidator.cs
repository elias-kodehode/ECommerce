using ECommerce.Api.Features.Users.Common;
using FluentValidation;

namespace ECommerce.Api.Features.Users.LoginUser;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
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
    }
}
