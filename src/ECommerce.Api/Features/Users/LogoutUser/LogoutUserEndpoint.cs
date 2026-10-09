using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Features.Users.LogoutUser;

public class LogoutUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/logout", async (ICommandDispatcher dispatcher, CancellationToken ct) => {
            var result = await dispatcher.SendAsync(new LogoutCommand(), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest();
        });
    }
}
