using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Users.Common;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Features.Users.LoginUser;

public class LoginUserHandler(
    SignInManager<AppUser> signInManager,
    IValidator<LoginUserCommand> validator) : ICommandHandler<LoginUserCommand, Result>
{
    public async Task<Result> HandleAsync(LoginUserCommand command, CancellationToken ct)
    {

        var validationResult = await validator.ValidateAsync(command, ct);

        if (!validationResult.IsValid)
        {
            if(validationResult.Errors.Count <= 0)
            {
                Console.WriteLine();
            }
            Error[] errors = [.. validationResult.Errors.Select(failure => Error.Validation(failure.ErrorCode, failure.ErrorMessage))];

            return Result.Failure(errors);
        }


        var user = await signInManager.UserManager.FindByEmailAsync(command.Email);

        if(user is null)
        {
            return Result.Failure(UserErrors.NotAllowed);
        }
        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, false);



        if (!result.Succeeded)
        {
            return Result.Failure(Error.Unauthorized("something wrong", "something wrong"));
        }

        await signInManager.SignInAsync(user, true, CookieAuthenticationDefaults.AuthenticationScheme);
        return Result.Success();
    }
}
