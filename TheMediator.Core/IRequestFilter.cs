namespace TheMediator.Core;

public interface IRequestFilter
{
    Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
        where TRequest : notnull;
}