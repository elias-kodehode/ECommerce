namespace ECommerce.Api.Common.Messaging;

public interface IQueryDispatcher
{
    Task<TResponse> SendAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken ct = default);
}
