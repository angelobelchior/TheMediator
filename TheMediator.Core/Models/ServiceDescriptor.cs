namespace TheMediator.Core.Models;

[ExcludeFromCodeCoverage]
public abstract record ServiceDescriptor
{
    protected ServiceDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type mainType)
        => MainType = mainType;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type MainType { get; }
}

[ExcludeFromCodeCoverage]
public sealed record HandlerDescriptor : ServiceDescriptor
{
    public HandlerDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type mainType,
        Type requestType,
        Type? responseType)
        : base(mainType)
    {
        RequestType = requestType;
        ResponseType = responseType;
    }

    public Type RequestType { get; }
    public Type? ResponseType { get; }
}

[ExcludeFromCodeCoverage]
public sealed record FilterDescriptor : ServiceDescriptor
{
    public FilterDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type mainType,
        Type? requestType)
        : base(mainType)
        => RequestType = requestType;

    public Type? RequestType { get; }
}

[ExcludeFromCodeCoverage]
public sealed record NotificationDescriptor : ServiceDescriptor
{
    public NotificationDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type mainType,
        Type messageType)
        : base(mainType)
        => MessageType = messageType;

    public Type MessageType { get; }
}
