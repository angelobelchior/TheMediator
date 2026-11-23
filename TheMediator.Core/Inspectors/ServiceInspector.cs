namespace TheMediator.Core.Inspectors;

internal static class ServiceInspector
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(IRequestHandler<,>),
        typeof(IRequestHandler<>)
    ];

    private static readonly Type[] FilterInterfaces =
    [
        typeof(IRequestFilter),
        typeof(IRequestFilter<>),
    ];

    private static readonly Type[] NotificationInterfaces =
    [
        typeof(INotificationHandler<>)
    ];

    public static IEnumerable<Models.ServiceDescriptor> GetServicesByAssembly(Assembly assembly)
    {
        foreach (var type in GetValidTypesFromAssembly(assembly))
        foreach (var serviceDescriptor in GetServiceDescriptorsForType(type))
            yield return serviceDescriptor;
    }

    public static Models.HandlerDescriptor CreateHandlerDescriptor(Type mainType)
    {
        var requestResponseInterface = GetMatchingInterface(mainType, typeof(IRequestHandler<,>));
        if (requestResponseInterface is not null)
            return CreateHandlerDescriptor(mainType, requestResponseInterface);

        var requestInterface = GetMatchingInterface(mainType, typeof(IRequestHandler<>));
        if (requestInterface is not null)
            return CreateHandlerDescriptor(mainType, requestInterface);

        throw new InvalidOperationException(
            $"The type {mainType} does not implement a valid handler interface.");
    }

    public static Models.FilterDescriptor CreateFilterDescriptor(Type mainType)
    {
        var requestInterface = GetMatchingInterface(mainType, typeof(IRequestFilter<>));
        if (requestInterface is not null)
            return CreateFilterDescriptor(mainType, requestInterface);

        var nonGenericInterface = GetMatchingInterface(mainType, typeof(IRequestFilter));
        if (nonGenericInterface is not null)
            return CreateFilterDescriptor(mainType, nonGenericInterface);

        throw new InvalidOperationException(
            $"The type {mainType} does not implement a valid filter interface.");
    }

    public static Models.NotificationDescriptor CreateNotificationDescriptor(Type mainType)
    {
        var interfaceType = GetMatchingInterface(mainType, typeof(INotificationHandler<>));
        if (interfaceType is not null)
            return CreateNotificationDescriptor(mainType, interfaceType);

        throw new InvalidOperationException(
            $"The type {mainType} does not implement a valid notification interface.");
    }

    internal static IEnumerable<Type> GetValidTypesFromAssembly(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => t.IsClass && t is { IsAbstract: false, IsGenericType: false });

    internal static IEnumerable<Models.ServiceDescriptor> GetServiceDescriptorsForType(Type type)
    {
        foreach (var @interface in type.GetInterfaces())
        {
            var genericType = @interface.IsGenericType ? @interface.GetGenericTypeDefinition() : @interface;

            if (HandlerInterfaces.Contains(genericType))
                yield return CreateHandlerDescriptor(type, @interface);
            else if (FilterInterfaces.Contains(genericType))
                yield return CreateFilterDescriptor(type, @interface);
            else if (NotificationInterfaces.Contains(genericType))
                yield return CreateNotificationDescriptor(type, @interface);
        }
    }

    internal static Type? GetMatchingInterface(Type mainType, Type type)
    {
        var allInterfaces = mainType.GetInterfaces();

        return allInterfaces.FirstOrDefault(i =>
                   i.IsGenericType && i.GetGenericTypeDefinition() == type) ??
               allInterfaces.FirstOrDefault(i => i == type);
    }

    private static Models.HandlerDescriptor CreateHandlerDescriptor(Type mainType, Type interfaceType)
    {
        var arguments = interfaceType.GetGenericArguments();
        return arguments.Length == 2
            ? new Models.HandlerDescriptor(mainType, arguments[0], arguments[1])
            : new Models.HandlerDescriptor(mainType, arguments[0], null);
    }

    private static Models.FilterDescriptor CreateFilterDescriptor(Type mainType, Type interfaceType)
        => interfaceType.IsGenericType
            ? new Models.FilterDescriptor(mainType, interfaceType.GetGenericArguments()[0])
            : new Models.FilterDescriptor(mainType, null);

    private static Models.NotificationDescriptor CreateNotificationDescriptor(Type mainType,
        Type interfaceType)
        => new(mainType, interfaceType.GetGenericArguments()[0]);
}
