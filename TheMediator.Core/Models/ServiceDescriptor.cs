namespace TheMediator.Core.Models;

internal record ServiceDescriptor(
    Type MainType,
    Type RequestType,
    Type ResponseType,
    ServiceCategory Category);