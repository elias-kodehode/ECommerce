using Scalar.AspNetCore;

namespace ECommerce.Api.Common.OpenApi;

public static class OpenApiExtensions
{
    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi();
        app.MapScalarApiReference(options => options
            .WithTitle("ECommerce API")
            .ShowOperationId()
            .DisableAgent());

        return app;
    }
}
