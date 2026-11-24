namespace TheMediator.Core.Registries;

public class HandlerRegistry : ServiceRegistry<HandlerDescriptor>
{
    public void Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler,
        TRequest,
        TResponse>()
        where THandler : IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
        => Add(new HandlerDescriptor(typeof(THandler), typeof(TRequest), typeof(TResponse)));

    public void Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler,
        TRequest>()
        where THandler : IRequestHandler<TRequest>
        where TRequest : notnull
        => Add(new HandlerDescriptor(typeof(THandler), typeof(TRequest), null));

    private readonly HashSet<HandlerDescriptor> _handlers = new();

    protected override IEnumerable<HandlerDescriptor> Entries => _handlers;

    protected override void EnsureUnique(HandlerDescriptor descriptor)
    {
        var current = _handlers.FirstOrDefault(s => s.RequestType == descriptor.RequestType &&
                                                    s.ResponseType == descriptor.ResponseType);
        if (current is not null)
            throw new InvalidOperationException(
                $"Cannot register {descriptor.MainType}!.\n" +
                $"The handler {current.MainType} is already registered with the same request {descriptor.RequestType} and response {descriptor.ResponseType} types.");
    }

    protected override void Insert(HandlerDescriptor descriptor) => _handlers.Add(descriptor);

    internal HandlerDescriptor GetHandler<TRequest, TResponse>()
    {
        var type = _handlers.FirstOrDefault(s => s.RequestType == typeof(TRequest) &&
                                                 s.ResponseType == typeof(TResponse));
        if (type is null)
            throw new InvalidOperationException(
                $"No Handler found for request type {typeof(TRequest)} and response type {typeof(TResponse)}.");

        return type;
    }

    internal HandlerDescriptor GetHandler<TRequest>()
    {
        var type = _handlers.FirstOrDefault(s => s.RequestType == typeof(TRequest) &&
                                                 s.ResponseType is null);
        if (type is null)
            throw new InvalidOperationException(
                $"No Handler found for request type {typeof(TRequest)}.");

        return type;
    }
}
