using TheMediator.Core.Registries;

namespace TheMediator.Core.Executors;

internal class FilterExecutor(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    FilterRegistry filterRegistry) : IDisposable
{
    private readonly ILogger logger = loggerFactory.CreateLogger(Constants.LogCategoryName);
    
    public Task Execute<TRequest>(
        TRequest request,
        Func<Task> function,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var task = filterRegistry.ListFiltersWithRequest<TRequest>()
            .Select(f => serviceProvider.GetRequiredService(f.MainType))
            .Aggregate(
                function,
                (acc, source) => () =>
                {
                    logger.LogTrace(
                        "Executing filter {Filter} for request {Request}",
                        source.GetType().Name,
                        request.GetType().Name);

                    cancellationToken.ThrowIfCancellationRequested();

                    var next = async () =>
                    {
                        await acc();
                        return Models.Void.Null;
                    };

                    var response = source switch
                    {
                        IRequestFilter filter => filter.FilterAsync(request, next, cancellationToken),
                        IRequestFilter<TRequest> requestFilter => requestFilter.FilterAsync(request, next,
                            cancellationToken),
                        _ => Task.CompletedTask
                    };

                    logger.LogTrace(
                        "Executed filter {Filter} for request {Request}",
                        source.GetType().Name,
                        request.GetType().Name);

                    return response;
                });
        return task();
    }

    public Task<TResponse> Execute<TRequest, TResponse>(
        TRequest request,
        Func<Task<TResponse>> function,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var task = filterRegistry.ListFiltersWithRequest<TRequest>()
            .Select(f => serviceProvider.GetRequiredService(f.MainType))
            .Aggregate(
                function,
                (acc, source) => () =>
                {
                    logger.LogTrace(
                        "Executing filter {Filter} for request {Request}",
                        source.GetType().Name,
                        request.GetType().Name);

                    cancellationToken.ThrowIfCancellationRequested();

                    var response = source switch
                    {
                        IRequestFilter filter => filter.FilterAsync(request, acc, cancellationToken),
                        IRequestFilter<TRequest> requestFilter => requestFilter.FilterAsync(request, acc,
                            cancellationToken),
                        _ => throw new InvalidOperationException(
                            $"Filter {source.GetType().Name} does not implement IRequestFilter or IRequestFilter<{typeof(TRequest).Name}>")
                    };

                    logger.LogTrace(
                        "Executed filter {Filter} for request {Request}",
                        source.GetType().Name,
                        request.GetType().Name);

                    return response;
                });
        return task();
    }

    public void Dispose()
        => loggerFactory.Dispose();
}