using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Features.Users.Common;

namespace ECommerce.Api.Features.Users.RegisterUser;

public class RegisterUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/register", async (RegisterUserRequest request, ICommandDispatcher dispatcher) =>
        {
            var result = await dispatcher.SendAsync(new RegisterUserCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword
                ));

            return result.Match(
                onSuccess: user => Results.Ok(user.Id),
                onFailure: errors => errors.ToProblemDetails()
            );
        })
        .WithName("RegisterUser")
        .WithSummary("Register a User")
        .WithDescription("Register a new user. Email must be unique")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
