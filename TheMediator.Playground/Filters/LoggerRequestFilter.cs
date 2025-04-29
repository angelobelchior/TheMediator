using TheMediator.Core;

namespace TheMediator.Playground.Filters;

public class LoggerRequestFilter(ILogger<LoggerRequestFilter> logger)
    : IRequestFilter
{
    public Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        logger.LogInformation("{Type} execution started: {Time}", request.GetType().Name, DateTime.Now);
        var response = next();
        logger.LogInformation("{Type} execution finished: {Time}", request.GetType().Name, DateTime.Now);
        return response;
    }
}