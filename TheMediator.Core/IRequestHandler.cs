namespace TheMediator.Core;

/// <summary>
/// Represents a filter that can be used to intercept and modify requests and responses in the mediator pipeline.
/// </summary>
/// <typeparam name="TRequest">Request Type</typeparam>
/// <typeparam name="TResponse">Response Type</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Response</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a filter that can be used to intercept and modify requests in the mediator pipeline.
/// </summary>
/// <typeparam name="TRequest">Request Type</typeparam>
public interface IRequestHandler<in TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Task</returns>
    Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}