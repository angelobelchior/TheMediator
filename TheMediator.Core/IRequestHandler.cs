namespace TheMediator.Core;

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest>
    where TRequest : notnull
{
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}