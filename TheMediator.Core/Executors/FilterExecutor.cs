using TheMediator.Core.Registries;

namespace TheMediator.Core.Executors;

internal partial class FilterExecutor(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    FilterRegistry filterRegistry)
{
    private readonly ILogger logger = loggerFactory.CreateLogger(Constants.LogCategoryName);

    [LoggerMessage(LogLevel.Trace, "Executing filter {Filter} for request {Request}")]
    private static partial void LogExecutingFilter(ILogger logger, string filter, string request);

    [LoggerMessage(LogLevel.Trace, "Executed filter {Filter} for request {Request}")]
    private static partial void LogExecutedFilter(ILogger logger, string filter, string request);

    public Task Execute<TRequest>(
        IRequestHandler<TRequest> handler,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var filters = filterRegistry.ListFiltersWithRequest<TRequest>();
        if (filters.Count == 0)
            return handler.HandleAsync(request, cancellationToken);

        Func<Task<Models.Void>> function = async () =>
        {
            await handler.HandleAsync(request, cancellationToken);
            return Models.Void.Null;
        };

        foreach (var descriptor in filters)
        {
            var source = serviceProvider.GetRequiredService(descriptor.MainType);
            var next = function;
            function = () => InvokeFilter<TRequest, Models.Void>(source, request, next, cancellationToken);
        }

        return function();
    }

    public Task<TResponse> Execute<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> handler,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var filters = filterRegistry.ListFiltersWithRequest<TRequest>();
        if (filters.Count == 0)
            return handler.HandleAsync(request, cancellationToken);

        Func<Task<TResponse>> function = () => handler.HandleAsync(request, cancellationToken);

        foreach (var descriptor in filters)
        {
            var source = serviceProvider.GetRequiredService(descriptor.MainType);
            var next = function;
            function = () => InvokeFilter<TRequest, TResponse>(source, request, next, cancellationToken);
        }

        return function();
    }

    private Task<TResponse> InvokeFilter<TRequest, TResponse>(
        object source,
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        LogExecutingFilter(logger, source.GetType().Name, request.GetType().Name);
        cancellationToken.ThrowIfCancellationRequested();

        var response = source switch
        {
            IRequestFilter filter => filter.FilterAsync<TRequest, TResponse>(request, next, cancellationToken),
            IRequestFilter<TRequest> requestFilter => requestFilter.FilterAsync(request, next, cancellationToken),
            _ => throw new InvalidOperationException(
                $"Filter {source.GetType().Name} does not implement IRequestFilter or IRequestFilter<{typeof(TRequest).Name}>")
        };

        LogExecutedFilter(logger, source.GetType().Name, request.GetType().Name);

        return response;
    }
}
