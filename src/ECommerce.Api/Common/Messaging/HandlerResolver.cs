using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Api.Common.Messaging;

internal static class HandlerResolver
{
    public static THandler GetSingle<THandler>(
        IServiceProvider serviceProvider,
        Type requestType)
        where THandler : notnull
    {
        using IEnumerator<THandler> handlers = serviceProvider
            .GetServices<THandler>()
            .GetEnumerator();

        if (!handlers.MoveNext())
        {
            throw new InvalidOperationException(
                $"No handler is registered for '{requestType.FullName}'. " +
                $"Expected one registration for '{typeof(THandler).FullName}'.");
        }

        THandler handler = handlers.Current;

        if (handlers.MoveNext())
        {
            throw new InvalidOperationException(
                $"Multiple handlers are registered for '{requestType.FullName}'. " +
                "Each command or query must have exactly one handler.");
        }

        return handler;
    }
}
