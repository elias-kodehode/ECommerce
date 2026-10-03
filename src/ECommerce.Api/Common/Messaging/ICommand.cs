using ECommerce.Api.Common.Results;

namespace ECommerce.Api.Common.Messaging;

public interface ICommand : ICommand<Result>
{
}

public interface ICommand<TResponse>
{
}
