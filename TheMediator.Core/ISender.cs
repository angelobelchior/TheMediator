using TheMediator.Core.Executors;

namespace TheMediator.Core;

public interface ISender
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull;

    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull;
}

internal class Sender(HandlerExecutor handlerExecutor) : ISender
{
    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
        => handlerExecutor.SendAsync<TRequest, TResponse>(request, cancellationToken);

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull
        => handlerExecutor.SendAsync(request, cancellationToken);
}