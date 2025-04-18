using TheMediator.Core.Registries;

namespace TheMediator.Core.Executors;

internal class FilterExecutor(
    IServiceProvider serviceProvider,
    FilterRegistry filterRegistry)
{
    public Task Execute<TRequest>(
        TRequest request,
        Func<Task> function,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var task = filterRegistry.ListFilters()
            .Select(f => (IRequestFilter)serviceProvider.GetRequiredService(f.MainType))
            .Aggregate(
                function,
                (acc, source) => () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return source.FilterAsync(request, acc, cancellationToken);
                });
        return task();
    }

    public Task<TResponse> Execute<TRequest, TResponse>(
        TRequest request,
        Func<Task<TResponse>> function,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var task = filterRegistry.ListFilters()
            .Select(f => (IRequestFilter)serviceProvider.GetRequiredService(f.MainType))
            .Aggregate(
                function,
                (acc, source) => async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TResponse response = default!;
                    await source.FilterAsync(request, async () => response = await acc(), cancellationToken);
                    return response;
                });
        return task();
    }
}