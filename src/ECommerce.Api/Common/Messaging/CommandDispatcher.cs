using System.Collections.Concurrent;

namespace ECommerce.Api.Common.Messaging;

internal sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return CommandInvokerCache<TResponse>
            .GetOrCreate(command.GetType())
            .InvokeAsync(serviceProvider, command, ct);
    }

    private interface ICommandInvoker<TResponse>
    {
        Task<TResponse> InvokeAsync(
            IServiceProvider services,
            ICommand<TResponse> command,
            CancellationToken ct);
    }

    private sealed class CommandInvoker<TCommand, TResponse> : ICommandInvoker<TResponse>
        where TCommand : ICommand<TResponse>
    {
        public Task<TResponse> InvokeAsync(
            IServiceProvider services,
            ICommand<TResponse> command,
            CancellationToken ct)
        {
            ICommandHandler<TCommand, TResponse> handler =
                HandlerResolver.GetSingle<ICommandHandler<TCommand, TResponse>>(
                    services,
                    typeof(TCommand));

            return handler.HandleAsync((TCommand)command, ct);
        }
    }

    private static class CommandInvokerCache<TResponse>
    {
        private static readonly ConcurrentDictionary<Type, ICommandInvoker<TResponse>> Invokers = new();

        public static ICommandInvoker<TResponse> GetOrCreate(Type commandType) =>
            Invokers.GetOrAdd(commandType, Create);

        private static ICommandInvoker<TResponse> Create(Type commandType)
        {
            Type invokerType = typeof(CommandInvoker<,>).MakeGenericType(commandType, typeof(TResponse));

            return (ICommandInvoker<TResponse>)(Activator.CreateInstance(invokerType)
                ?? throw new InvalidOperationException(
                    $"Could not create a command invoker for '{commandType.FullName}'."));
        }
    }
}
