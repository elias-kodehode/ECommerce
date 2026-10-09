using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Features.Users.LogoutUser;

public class LogoutCommandHandler(
    SignInManager<AppUser> signInManager
    ) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken ct)
    {
        await signInManager.SignOutAsync();
        return Result.Success();
    }
}
