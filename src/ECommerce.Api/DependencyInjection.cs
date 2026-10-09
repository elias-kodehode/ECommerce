using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Data;
using ECommerce.Api.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser,CurrentUser>();


        services.AddIdentity<AppUser, IdentityRole>(x =>
        {
            x.Password.RequireNonAlphanumeric = false;
            x.Password.RequiredLength = 4;
            x.Password.RequireUppercase = false;
            x.Password.RequireDigit = false;
            x.User.RequireUniqueEmail = true;
            x.SignIn.RequireConfirmedEmail = false;
        })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<AppDbContext>();

        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                return Task.CompletedTask;
            };
        });


        services.AddCqrs(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddOpenApi();
        services.AddMemoryCache();

        return services;
    }
}
