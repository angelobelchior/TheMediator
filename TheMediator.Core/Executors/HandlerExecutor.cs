using Microsoft.Extensions.Logging;
using TheMediator.Core.Models;
using TheMediator.Core.Registries;
using Void = TheMediator.Core.Models.Void;

namespace TheMediator.Core.Executors;

internal class HandlerExecutor(
    ILogger<HandlerExecutor> logger,
    IServiceProvider serviceProvider,
    HandlerRegistry handlerRegistry,
    FilterExecutor filterExecutor)
{
    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var handlerType = handlerRegistry.GetHandler<TRequest, TResponse>(ServiceCategory.Handler);
        logger.LogInformation("Executing handler {HandlerType} for request type {RequestType} and response type {ResponseType}", 
            handlerType.MainType.Name,
            typeof(TRequest).Name,
            typeof(TResponse).Name);
        
        cancellationToken.ThrowIfCancellationRequested();

        var handlerService =
            (IRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(handlerType.MainType);

        return filterExecutor.Execute(
            request,
            () => handlerService.HandleAsync(request, cancellationToken),
            cancellationToken);
    }

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var handlerType = handlerRegistry.GetHandler<TRequest, Void>(ServiceCategory.Handler);
        logger.LogInformation("Executing handler {HandlerType} for request type {RequestType}", 
            handlerType.MainType.Name,
            typeof(TRequest).Name);
        
        cancellationToken.ThrowIfCancellationRequested();
        
        var handlerService = (IRequestHandler<TRequest>)serviceProvider.GetRequiredService(handlerType.MainType);

        return filterExecutor.Execute(
            request,
            () => handlerService.HandleAsync(request, cancellationToken),
            cancellationToken);
    }
}