using System.Diagnostics;
using TheMediator.Core;

namespace TheMediator.Playground.Application;

public class MeasureTimeRequestFilter(ILogger<MeasureTimeRequestFilter> logger)
    : IRequestFilter
{
    public Task<TResponse> FilterAsync<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> next,
        CancellationToken cancellationToken) where TRequest : notnull
    {
        logger.LogInformation("Started measuring time for {Type} at {Time}", request.GetType().Name, DateTime.Now);
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var response = next();

        stopwatch.Stop();
        logger.LogInformation("{Type} execution time: {Time} ms", request.GetType().Name, stopwatch.ElapsedMilliseconds);

        return response;
    }
}