using TheMediator.Core.Inspectors;
using TheMediator.Core.Models;
using ServiceDescriptor = TheMediator.Core.Models.ServiceDescriptor;

namespace TheMediator.Core.Registries;

public class HandlerRegistry(IServiceCollection services)
{
    private readonly HashSet<ServiceDescriptor> _handlers = new();
    
    public void Add<THandler>()
        where THandler : notnull
        => Add<THandler>(ServiceCategory.Handler);

    internal void Add<TService>(ServiceCategory serviceCategory)
        where TService : notnull
    {
        var mainType = typeof(TService);
        var service = ServiceInspector.CreateService(mainType, serviceCategory);
        Add(service);
    }

    internal void Add(ServiceDescriptor serviceDescriptor)
    {
        var current = _handlers.FirstOrDefault(s => s.RequestType == serviceDescriptor.RequestType &&
                                                    s.ResponseType == serviceDescriptor.ResponseType &&
                                                    s.Category == serviceDescriptor.Category);
        if (current is not null)
            throw new InvalidOperationException(
                $"Cannot register {serviceDescriptor.MainType}!.\n" +
                $"The handler {current.MainType} is already registered with the same request {serviceDescriptor.RequestType} and response {serviceDescriptor.ResponseType} types.");

        _handlers.Add(serviceDescriptor);
        services.AddSingleton(serviceDescriptor.MainType);
    }

    internal ServiceDescriptor GetHandler<TRequest, TResponse>(ServiceCategory serviceCategory)
    {
        var type = _handlers.FirstOrDefault(s => s.RequestType == typeof(TRequest) &&
                                                 s.ResponseType == typeof(TResponse));
        if (type is null)
            throw new InvalidOperationException(
                $"No {serviceCategory} found for request type {typeof(TRequest)} and response type {typeof(TResponse)}.");

        return type;
    }
}