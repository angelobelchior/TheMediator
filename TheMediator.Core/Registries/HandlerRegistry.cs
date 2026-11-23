using TheMediator.Core.Inspectors;

namespace TheMediator.Core.Registries;

public class HandlerRegistry : ServiceRegistry<Models.HandlerDescriptor>
{
    public HandlerRegistry()
        : base(ServiceInspector.CreateHandlerDescriptor)
    {
    }

    private readonly HashSet<Models.HandlerDescriptor> _handlers = new();

    protected override IEnumerable<Models.HandlerDescriptor> Entries => _handlers;

    protected override void EnsureUnique(Models.HandlerDescriptor descriptor)
    {
        var current = _handlers.FirstOrDefault(s => s.RequestType == descriptor.RequestType &&
                                                    s.ResponseType == descriptor.ResponseType);
        if (current is not null)
            throw new InvalidOperationException(
                $"Cannot register {descriptor.MainType}!.\n" +
                $"The handler {current.MainType} is already registered with the same request {descriptor.RequestType} and response {descriptor.ResponseType} types.");
    }

    protected override void Insert(Models.HandlerDescriptor descriptor) => _handlers.Add(descriptor);

    internal Models.HandlerDescriptor GetHandler<TRequest, TResponse>()
    {
        var type = _handlers.FirstOrDefault(s => s.RequestType == typeof(TRequest) &&
                                                 s.ResponseType == typeof(TResponse));
        if (type is null)
            throw new InvalidOperationException(
                $"No Handler found for request type {typeof(TRequest)} and response type {typeof(TResponse)}.");

        return type;
    }

    internal Models.HandlerDescriptor GetHandler<TRequest>()
    {
        var type = _handlers.FirstOrDefault(s => s.RequestType == typeof(TRequest) &&
                                                 s.ResponseType is null);
        if (type is null)
            throw new InvalidOperationException(
                $"No Handler found for request type {typeof(TRequest)}.");

        return type;
    }
}
