using TheMediator.Core.Registries;
using Void = TheMediator.Core.Models.Void;

namespace TheMediator.Core.Executors;

internal class HandlerExecutor(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    HandlerRegistry handlerRegistry,
    FilterExecutor filterExecutor)
{
    private readonly ILogger logger = loggerFactory.CreateLogger(Constants.LogCategoryName);

    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var handlerType = handlerRegistry.GetHandler<TRequest, TResponse>();
        logger.LogTrace(
            "Executing handler {HandlerType} for request type {RequestType} and response type {ResponseType}",
            handlerType.MainType.Name,
            typeof(TRequest).Name,
            typeof(TResponse).Name);

        cancellationToken.ThrowIfCancellationRequested();

        var handlerService =
            (IRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(handlerType.MainType);

        var response = filterExecutor.Execute(
            request,
            () => handlerService.HandleAsync(request, cancellationToken),
            cancellationToken);

        logger.LogTrace(
            "Handler {HandlerType} for request type {RequestType} and response type {ResponseType} executed successfully",
            handlerType.MainType.Name,
            typeof(TRequest).Name,
            typeof(TResponse).Name);

        return response;
    }

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var handlerType = handlerRegistry.GetHandler<TRequest, Void>();
        logger.LogTrace("Executing handler {HandlerType} for request type {RequestType}",
            handlerType.MainType.Name,
            typeof(TRequest).Name);

        cancellationToken.ThrowIfCancellationRequested();

        var handlerService = (IRequestHandler<TRequest>)serviceProvider.GetRequiredService(handlerType.MainType);

        var response = filterExecutor.Execute(
            request,
            () => handlerService.HandleAsync(request, cancellationToken),
            cancellationToken);

        logger.LogTrace("Handler {HandlerType} for request type {RequestType} executed successfully",
            handlerType.MainType.Name,
            typeof(TRequest).Name);

        return response;
    }
}