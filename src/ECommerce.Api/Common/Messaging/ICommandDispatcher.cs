namespace ECommerce.Api.Common.Messaging;

public interface ICommandDispatcher
{
    Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken ct = default);
}
