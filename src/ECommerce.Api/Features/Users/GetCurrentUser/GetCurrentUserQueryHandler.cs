using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Common.Results;
using ECommerce.Api.Data;
using ECommerce.Api.Domain;
using ECommerce.Api.Features.Users.Common;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Features.Users.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    UserManager<AppUser> userManager,
    ICurrentUser currentUser) : IQueryHandler<GetCurrentUserQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(GetCurrentUserQuery query, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(currentUser.UserId))
        {
            return Result<UserResponse>.Failure(Error.Validation("User.Id.Missing", "User ID is missing"));
        }

        var user = await userManager.FindByIdAsync(currentUser.UserId);


        if (user is null)
        {
            return Result<UserResponse>.Failure(Error.Unauthorized("User.NotFound", "User Is not found."));
        }
        return Result<UserResponse>.Success(new UserResponse(user.Id));
    }
}
