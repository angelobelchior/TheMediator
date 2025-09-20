using TheMediator.Core.Inspectors;
using ServiceDescriptor = TheMediator.Core.Models.ServiceDescriptor;

namespace TheMediator.Core.Registries;

/// <summary>
/// Handler registry.
/// </summary>
/// <param name="services">Service Collection</param>
public class HandlerRegistry(IServiceCollection services)
{
    private readonly HashSet<ServiceDescriptor> _handlers = new();

    /// <summary>
    /// Adds a handler to the registry.
    /// </summary>
    /// <typeparam name="TService">Service Type</typeparam>
    public void Add<TService>()
        where TService : notnull
    {
        var mainType = typeof(TService);
        var service = ServiceInspector.CreateService(mainType, ServiceCategory.Handler);
        Add(service);
    }

    internal void Add(ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.Category != ServiceCategory.Handler)
            throw new InvalidOperationException($"{serviceDescriptor.MainType} is not a handler type!");

        var current = _handlers.FirstOrDefault(s => s.RequestType == serviceDescriptor.RequestType &&
                                                    s.ResponseType == serviceDescriptor.ResponseType &&
                                                    s.Category == serviceDescriptor.Category);
        if (current is not null)
            throw new InvalidOperationException(
                $"Cannot register {serviceDescriptor.MainType}!.\n" +
                $"The handler {current.MainType} is already registered with the same request {serviceDescriptor.RequestType} and response {serviceDescriptor.ResponseType} types.");

        _handlers.Add(serviceDescriptor);
        services.AddScoped(serviceDescriptor.MainType);
    }

    internal ServiceDescriptor GetHandler<TRequest, TResponse>()
    {
        var type = _handlers.FirstOrDefault(s => s.RequestType == typeof(TRequest) &&
                                                 s.ResponseType == typeof(TResponse));
        if (type is null)
            throw new InvalidOperationException(
                $"No {ServiceCategory.Handler} found for request type {typeof(TRequest)} and response type {typeof(TResponse)}.");

        return type;
    }
}