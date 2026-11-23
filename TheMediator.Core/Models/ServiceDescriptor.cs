namespace TheMediator.Core.Models;

[ExcludeFromCodeCoverage]
public abstract record ServiceDescriptor(Type MainType);

[ExcludeFromCodeCoverage]
public sealed record HandlerDescriptor(Type MainType, Type RequestType, Type? ResponseType)
    : ServiceDescriptor(MainType);

[ExcludeFromCodeCoverage]
public sealed record FilterDescriptor(Type MainType, Type? RequestType)
    : ServiceDescriptor(MainType);

[ExcludeFromCodeCoverage]
public sealed record NotificationDescriptor(Type MainType, Type MessageType)
    : ServiceDescriptor(MainType);
