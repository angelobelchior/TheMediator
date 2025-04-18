using TheMediator.Core.Models;
using TheMediator.Core.Registries;
using Void = TheMediator.Core.Inspectors.Void;

namespace TheMediator.Core.Executors;

internal class HandlerExecutor(
    IServiceProvider serviceProvider,
    HandlerRegistry handlerRegistry,
    FilterExecutor filterExecutor)
{
    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();

        var handlerType = handlerRegistry.GetHandler<TRequest, TResponse>(ServiceCategory.Handler);
        var handlerService = (IRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(handlerType.MainType);

        return filterExecutor.Execute(
            request,
            () => handlerService.HandleAsync(request, cancellationToken),
            cancellationToken);
    }

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();

        var handlerType = handlerRegistry.GetHandler<TRequest, Void>(ServiceCategory.Handler);
        var handlerService = (IRequestHandler<TRequest>)serviceProvider.GetRequiredService(handlerType.MainType);

        return filterExecutor.Execute(
            request,
            () => handlerService.HandleAsync(request, cancellationToken),
            cancellationToken);
    }
}