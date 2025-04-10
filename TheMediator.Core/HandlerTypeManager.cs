using System.Reflection;
using InvalidOperationException = System.InvalidOperationException;

namespace TheMediator.Core;

public class HandlerTypeManager(IServiceCollection services)
{
    private readonly HashSet<ServiceTypes> _handlersServiceTypes = new();

    public void AddServicesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
            throw new ArgumentNullException(nameof(assemblies), "Assemblies cannot be null or empty.");

        var handlerTypes = assemblies
            .Select(assembly => TypeInspector.GetTypesByAssembly(assembly, typeof(IHandler<,>), typeof(IHandler<>)))
            .SelectMany(handlers => handlers);

        foreach (var handlerType in handlerTypes)
            AddHandler(handlerType);
    }

    internal Type GetHandler<TRequest, TResponse>()
        => GetHandlerTypeByRequestResponse<TRequest, TResponse>();

    internal Type GetHandler<TRequest>()
        => GetHandlerTypeByRequestResponse<TRequest, Void>();
    
    private void AddHandler(Type handlerType)
    {
        var handlerServiceTypes = TypeInspector.GetServiceTypes(handlerType, typeof(IHandler<,>), typeof(IHandler<>));
        
        var notExists =  _handlersServiceTypes.Add(handlerServiceTypes);
        if(!notExists)
            throw new InvalidOperationException(
                $"Cannot register {handlerType}!.\nThe handler {handlerServiceTypes.MainType} is already registered with the same request {handlerServiceTypes.RequestType} and response {handlerServiceTypes.ResponseType} types.");

        services.AddSingleton(handlerType);
    }
    
    private Type GetHandlerTypeByRequestResponse<TRequest, TResponse>()
    {
        var requestType = typeof(TRequest);
        var responseType = typeof(TResponse);
        var type = _handlersServiceTypes.FirstOrDefault(h => h.RequestType == requestType && h.ResponseType == responseType);
        if (type is null)
            throw new InvalidOperationException(
                $"No handler found for request type {typeof(TRequest)} and response type {typeof(TResponse)}.");

        return type.MainType;
    }
}