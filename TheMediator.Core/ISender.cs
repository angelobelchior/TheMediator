namespace TheMediator.Core;

/// <summary>
/// Sender interface.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends a request to the appropriate handler and returns the response.
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <typeparam name="TRequest">Request Type</typeparam>
    /// <typeparam name="TResponse">Response Type</typeparam>
    /// <returns>Response</returns>
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull;

    /// <summary>
    /// Sends a request to the appropriate handler and returns the response.
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <typeparam name="TRequest">Request Type</typeparam>
    /// <returns>Task</returns>
    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull;
}
