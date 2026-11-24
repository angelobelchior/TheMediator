using TheMediator.Core.Registries;

namespace TheMediator.Core.Executors;

internal partial class HandlerExecutor(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    HandlerRegistry handlerRegistry,
    FilterExecutor filterExecutor) : ISender
{
    private readonly ILogger logger = loggerFactory.CreateLogger(Constants.LogCategoryName);

    [LoggerMessage(LogLevel.Trace, "Executing handler {HandlerType} for request type {RequestType} and response type {ResponseType}")]
    private static partial void LogExecuting(ILogger logger, string handlerType, string requestType, string responseType);

    [LoggerMessage(LogLevel.Trace, "Handler {HandlerType} for request type {RequestType} and response type {ResponseType} executed successfully")]
    private static partial void LogExecuted(ILogger logger, string handlerType, string requestType, string responseType);

    [LoggerMessage(LogLevel.Trace, "Executing handler {HandlerType} for request type {RequestType}")]
    private static partial void LogExecutingVoid(ILogger logger, string handlerType, string requestType);

    [LoggerMessage(LogLevel.Trace, "Handler {HandlerType} for request type {RequestType} executed successfully")]
    private static partial void LogExecutedVoid(ILogger logger, string handlerType, string requestType);

    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var handlerType = handlerRegistry.GetHandler<TRequest, TResponse>();
        LogExecuting(logger, handlerType.MainType.Name, typeof(TRequest).Name, typeof(TResponse).Name);

        cancellationToken.ThrowIfCancellationRequested();

        var handlerService =
            (IRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(handlerType.MainType);

        var response = filterExecutor.Execute(handlerService, request, cancellationToken);

        LogExecuted(logger, handlerType.MainType.Name, typeof(TRequest).Name, typeof(TResponse).Name);

        return response;
    }

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var handlerType = handlerRegistry.GetHandler<TRequest>();
        LogExecutingVoid(logger, handlerType.MainType.Name, typeof(TRequest).Name);

        cancellationToken.ThrowIfCancellationRequested();

        var handlerService = (IRequestHandler<TRequest>)serviceProvider.GetRequiredService(handlerType.MainType);

        var response = filterExecutor.Execute(handlerService, request, cancellationToken);

        LogExecutedVoid(logger, handlerType.MainType.Name, typeof(TRequest).Name);

        return response;
    }
}
