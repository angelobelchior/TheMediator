namespace TheMediator.Core.Models;

[ExcludeFromCodeCoverage]
internal record ServiceDescriptor(
    Type MainType,
    Type RequestType,
    Type ResponseType,
    ServiceCategory Category);