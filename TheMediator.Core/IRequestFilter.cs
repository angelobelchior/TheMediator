namespace TheMediator.Core;

/// <summary>
/// Represents a filter that can be used to intercept and modify requests and responses in the mediator pipeline.
/// </summary>
public interface IRequestFilter
{
    /// <summary>
    /// Filters the request and response in the mediator pipeline.
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="next">Next function in the pipeline</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <typeparam name="TRequest">Request Type</typeparam>
    /// <typeparam name="TResponse">Response Type</typeparam>
    /// <returns>Response</returns>
    Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
        where TRequest : notnull;
}

/// <summary>
/// Represents a filter that can be used to intercept and modify requests in the mediator pipeline.
/// </summary>
public interface IRequestFilter<in TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// Filters the request in the mediator pipeline.
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="next">Next function in the pipeline</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <typeparam name="TRequest">Request Type</typeparam>
    /// <typeparam name="TResponse">Response Type</typeparam>
    /// <returns>Response</returns>
    Task<TResponse> FilterAsync<TResponse>(TRequest request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken);
}