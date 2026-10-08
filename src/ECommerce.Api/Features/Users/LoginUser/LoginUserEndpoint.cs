using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;

namespace ECommerce.Api.Features.Users.LoginUser;

public class LoginUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/login", async (LoginUserRequest request,ICommandDispatcher dispatcher) => {
            
            var result = await dispatcher.SendAsync(new LoginUserCommand(
                Email: request.Email,
                Password: request.Password
            ));

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: errors => errors.ToProblemDetails()
            );
        });
    }
}
