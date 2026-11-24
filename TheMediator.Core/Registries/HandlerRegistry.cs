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

    private readonly Dictionary<(Type RequestType, Type? ResponseType), HandlerDescriptor> _handlers = new();

    protected override IEnumerable<HandlerDescriptor> Entries => _handlers.Values;

    protected override void EnsureUnique(HandlerDescriptor descriptor)
    {
        if (_handlers.ContainsKey((descriptor.RequestType, descriptor.ResponseType)))
            throw new InvalidOperationException(
                $"Cannot register {descriptor.MainType}!.\n" +
                $"The handler for request {descriptor.RequestType} and response {descriptor.ResponseType} types is already registered.");
    }

    protected override void Insert(HandlerDescriptor descriptor)
        => _handlers[(descriptor.RequestType, descriptor.ResponseType)] = descriptor;

    internal HandlerDescriptor GetHandler<TRequest, TResponse>()
        => _handlers.TryGetValue((typeof(TRequest), typeof(TResponse)), out var descriptor)
            ? descriptor
            : throw new InvalidOperationException(
                $"No Handler found for request type {typeof(TRequest)} and response type {typeof(TResponse)}.");

    internal HandlerDescriptor GetHandler<TRequest>()
        => _handlers.TryGetValue((typeof(TRequest), null), out var descriptor)
            ? descriptor
            : throw new InvalidOperationException(
                $"No Handler found for request type {typeof(TRequest)}.");
}
