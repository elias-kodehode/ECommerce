using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.Api.Common.Messaging;

public static class DependencyInjection
{
    private static readonly Type[] HandlerInterfaceDefinitions =
    [
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>)
    ];

    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            throw new ArgumentException(
                "At least one assembly must be supplied for handler discovery.",
                nameof(assemblies));
        }

        if (assemblies.Any(static assembly => assembly is null))
        {
            throw new ArgumentException("Assemblies cannot contain null values.", nameof(assemblies));
        }

        services.TryAddScoped<ICommandDispatcher, CommandDispatcher>();
        services.TryAddScoped<IQueryDispatcher, QueryDispatcher>();

        HandlerRegistration[] registrations = assemblies
            .Distinct()
            .SelectMany(static assembly => assembly.DefinedTypes)
            .Where(static type => type is { IsAbstract: false, IsInterface: false } &&
                                  !type.ContainsGenericParameters)
            .SelectMany(static implementationType => implementationType
                .ImplementedInterfaces
                .Where(IsHandlerInterface)
                .Select(serviceType => new HandlerRegistration(
                    serviceType,
                    implementationType.AsType())))
            .ToArray();

        foreach (IGrouping<Type, HandlerRegistration> group in
                 registrations.GroupBy(static registration => registration.ServiceType))
        {
            Type[] implementationTypes = group
                .Select(static registration => registration.ImplementationType)
                .Distinct()
                .ToArray();

            if (implementationTypes.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple handlers were found for '{group.Key.FullName}': " +
                    $"{string.Join(", ", implementationTypes.Select(static type => type.FullName))}.");
            }

            if (services.Any(descriptor => descriptor.ServiceType == group.Key))
            {
                throw new InvalidOperationException(
                    $"A handler is already registered for '{group.Key.FullName}'.");
            }

            services.AddScoped(group.Key, implementationTypes[0]);
        }

        return services;
    }

    private static bool IsHandlerInterface(Type type) =>
        type.IsGenericType &&
        HandlerInterfaceDefinitions.Contains(type.GetGenericTypeDefinition());

    private sealed record HandlerRegistration(Type ServiceType, Type ImplementationType);
}
