using System.Collections.Concurrent;

namespace ECommerce.Api.Common.Messaging;

internal sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public Task<TResponse> SendAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return QueryInvokerCache<TResponse>
            .GetOrCreate(query.GetType())
            .InvokeAsync(serviceProvider, query, ct);
    }

    private interface IQueryInvoker<TResponse>
    {
        Task<TResponse> InvokeAsync(
            IServiceProvider services,
            IQuery<TResponse> query,
            CancellationToken ct);
    }

    private sealed class QueryInvoker<TQuery, TResponse> : IQueryInvoker<TResponse>
        where TQuery : IQuery<TResponse>
    {
        public Task<TResponse> InvokeAsync(
            IServiceProvider services,
            IQuery<TResponse> query,
            CancellationToken ct)
        {
            IQueryHandler<TQuery, TResponse> handler =
                HandlerResolver.GetSingle<IQueryHandler<TQuery, TResponse>>(
                    services,
                    typeof(TQuery));

            return handler.HandleAsync((TQuery)query, ct);
        }
    }

    private static class QueryInvokerCache<TResponse>
    {
        private static readonly ConcurrentDictionary<Type, IQueryInvoker<TResponse>> Invokers = new();

        public static IQueryInvoker<TResponse> GetOrCreate(Type queryType) =>
            Invokers.GetOrAdd(queryType, Create);

        private static IQueryInvoker<TResponse> Create(Type queryType)
        {
            Type invokerType = typeof(QueryInvoker<,>).MakeGenericType(queryType, typeof(TResponse));

            return (IQueryInvoker<TResponse>)(Activator.CreateInstance(invokerType)
                ?? throw new InvalidOperationException(
                    $"Could not create a query invoker for '{queryType.FullName}'."));
        }
    }
}
