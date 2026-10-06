using ECommerce.Api.Common.Messaging;
using FluentValidation;

namespace ECommerce.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddCqrs(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddOpenApi();
        services.AddMemoryCache();

        return services;
    }
}
