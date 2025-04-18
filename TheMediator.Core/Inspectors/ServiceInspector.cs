using TheMediator.Core.Models;

namespace TheMediator.Core.Inspectors;

internal class Void;

internal static class ServiceInspector
{
    private static readonly List<Type> HandlerTypes =
    [
        typeof(IRequestHandler<,>),
        typeof(IRequestHandler<>)
    ];

    private static readonly List<Type> FilterTypes =
    [
        typeof(IRequestFilter)
    ];
    

    public static IEnumerable<Models.ServiceDescriptor> GetServicesByAssembly(
        Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && t is { IsAbstract: false, IsGenericType: false });

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces().Where(i => i.IsGenericType);
            foreach (var @interface in interfaces)
            {
                var genericType = @interface.GetGenericTypeDefinition();
                if (HandlerTypes.Contains(genericType))
                    yield return CreateService(type, ServiceCategory.Handler);
                if (FilterTypes.Contains(genericType))
                    yield return CreateService(type, ServiceCategory.Filter);
                
                //TODO: Incluir Notifications
                // if (NotificationTypes.Contains(genericType))
                //     yield return CreateService(type, ServiceCategory.Notification);
            }
        }
    }
    
    public static Models.ServiceDescriptor CreateService(
        Type mainType,
        ServiceCategory serviceCategory)
    {
        
        var types = serviceCategory == ServiceCategory.Filter ? FilterTypes : HandlerTypes;

        foreach (var type in types)
        {
            var interfaceType = GetMatchingInterface(mainType, type);
            if (interfaceType is null)
                continue;

            return CreateServiceFromInterface(mainType, interfaceType, serviceCategory);
        }

        throw new InvalidOperationException(
            $"The type {mainType} does not implement a valid handler or filter handler interface.");
    }

    internal static Models.ServiceDescriptor CreateServiceFromInterface(
        Type mainType,
        Type interfaceType,
        ServiceCategory serviceCategory)
    {
        var @void = typeof(Void);
        var genericArguments = interfaceType.GetGenericArguments();

        return genericArguments.Length switch
        {
            0 => new Models.ServiceDescriptor(mainType, @void, @void, serviceCategory),
            1 => new Models.ServiceDescriptor(mainType, genericArguments[0], @void, serviceCategory),
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