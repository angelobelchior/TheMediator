using Void = TheMediator.Core.Models.Void;

namespace TheMediator.Core.Inspectors;

internal static class ServiceInspector
{
    private static readonly List<Type> HandlerTypes =
    [
        typeof(IRequestHandler<,>),
        typeof(IRequestHandler<>)
    ];

    private static readonly List<Type> FilterTypes =
    [
        typeof(IRequestFilter),
        typeof(IRequestFilter<>),
    ];

    private static readonly List<Type> NotificationTypes =
    [
        typeof(INotificationHandler<>)
    ];


    public static IEnumerable<Models.ServiceDescriptor> GetServicesByAssembly(Assembly assembly)
    {
        var types = GetValidTypesFromAssembly(assembly);
        foreach (var type in types)
        foreach (var serviceDescriptor in GetServiceDescriptorsForType(type))
            yield return serviceDescriptor;
    }

    public static Models.ServiceDescriptor CreateService(
        Type mainType,
        ServiceCategory serviceCategory)
    {
        var types = serviceCategory switch
        {
            ServiceCategory.Filter => FilterTypes,
            ServiceCategory.Handler => HandlerTypes,
            ServiceCategory.Notification => NotificationTypes,
            _ => throw new ArgumentOutOfRangeException(nameof(serviceCategory), serviceCategory, null)
        };

        foreach (var type in types)
        {
            var interfaceType = GetMatchingInterface(mainType, type);
            if (interfaceType is null) continue;

            return CreateServiceFromInterface(mainType, interfaceType, serviceCategory);
        }

        throw new InvalidOperationException(
            $"The type {mainType} does not implement a valid handler, notification or filter handler interface.");
    }

    internal static IEnumerable<Type> GetValidTypesFromAssembly(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => t.IsClass && t is { IsAbstract: false, IsGenericType: false });

    internal static IEnumerable<Models.ServiceDescriptor> GetServiceDescriptorsForType(Type type)
    {
        var interfaces = type.GetInterfaces();

        foreach (var @interface in interfaces)
        {
            var genericType = @interface.IsGenericType ? @interface.GetGenericTypeDefinition() : @interface;
            
            if (HandlerTypes.Contains(genericType))
                yield return CreateService(type, ServiceCategory.Handler);
            else if (FilterTypes.Contains(genericType))
                yield return CreateService(type, ServiceCategory.Filter);
            else if (NotificationTypes.Contains(genericType))
                yield return CreateService(type, ServiceCategory.Notification);
        }
    }

    internal static Models.ServiceDescriptor CreateServiceFromInterface(
        Type mainType,
        Type interfaceType,
        ServiceCategory serviceCategory)
    {
        var genericArguments = interfaceType.GetGenericArguments();

        return genericArguments.Length switch
        {
            0 => new Models.ServiceDescriptor(mainType, Void.Type, Void.Type, serviceCategory),
            1 => new Models.ServiceDescriptor(mainType, genericArguments[0], Void.Type, serviceCategory),
            _ => new Models.ServiceDescriptor(mainType, genericArguments[0], genericArguments[1], serviceCategory)
        };
    }

    internal static Type? GetMatchingInterface(Type mainType, Type type)
    {
        var allInterfaces = mainType.GetInterfaces();

        return allInterfaces.FirstOrDefault(i =>
                   i.IsGenericType && i.GetGenericTypeDefinition() == type) ??
               allInterfaces.FirstOrDefault(i => i == type);
    }
}