namespace TheMediator.Core;

public interface ISender
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken);
    
    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken);
}

internal class Sender(IServiceProvider serviceProvider, HandlerTypeManager handlerTypeManager) : ISender
{
    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
    {
        var handlerType = handlerTypeManager.GetHandler<TRequest, TResponse>();
        var service = (IHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(handlerType);
        return service.HandleAsync(request, cancellationToken);
    }

    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        var handlerType = handlerTypeManager.GetHandler<TRequest>();
        var service = (IHandler<TRequest>)serviceProvider.GetRequiredService(handlerType);
        return service.HandleAsync(request, cancellationToken);
    }
}