using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;

namespace ECommerce.Api.Features.Users.GetCurrentUser;

public class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me", async (
            IQueryDispatcher dispatcher, 
            CancellationToken ct) =>
        {
            var result = await dispatcher.SendAsync(new GetCurrentUserQuery(), ct);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: errors => errors.ToProblemDetails()
            );
        }).RequireAuthorization();
    }
}
