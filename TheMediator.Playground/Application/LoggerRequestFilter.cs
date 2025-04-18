using TheMediator.Core;

namespace TheMediator.Playground.Application;

public class LoggerRequestFilter(ILogger<LoggerRequestFilter> logger)
    : IRequestFilter
{
    public Task FilterAsync<TRequest>(TRequest request, Func<Task> next,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        logger.LogInformation("{Type} execution started: {Time}", request.GetType().Name, DateTime.Now);

        var response = next();

        logger.LogInformation("{Type} execution finished: {Time}", request.GetType().Name, DateTime.Now);
        return response;
    }
}