using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Users.Common;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Features.Users.RegisterUser;

public class RegisterUserHandler(
    UserManager<AppUser> userManager,
    IValidator<RegisterUserCommand> validator) : ICommandHandler<RegisterUserCommand, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(RegisterUserCommand command, CancellationToken ct)
    {

        var validationResult = await validator.ValidateAsync(command, ct);

        if (!validationResult.IsValid)
        {
            Error[] errors = [.. validationResult.Errors.Select(failure => Error.Validation(failure.ErrorCode, failure.ErrorMessage))];

            return Result<UserResponse>.Failure(errors);
        }

        var appUser = new AppUser { 
            Email = command.Email,
            UserName = command.Email,
            EmailConfirmed  = true
        };

        var result = await userManager.CreateAsync(appUser, command.Password);

        if (!result.Succeeded)
        {
            Error[] errors = [.. result.Errors.Select(failure => Error.Validation(failure.Code, failure.Description))];
            return Result<UserResponse>.Failure(errors);
        }

        return Result<UserResponse>.Success(new UserResponse(appUser.Id));
    }
}
