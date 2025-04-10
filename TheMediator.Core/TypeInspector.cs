using System.Reflection;

namespace TheMediator.Core;

internal class Void;

internal record ServiceTypes(Type MainType, Type RequestType, Type ResponseType);

internal static class TypeInspector
{
    public static IEnumerable<Type> GetTypesByAssembly(Assembly assembly, params Type[] types)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && t is { IsAbstract: false, IsGenericType: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                types.Contains(i.GetGenericTypeDefinition())));

        return handlerTypes;
    }

    public static ServiceTypes GetServiceTypes(
        Type mainType,
        params Type[] types)
    {
        foreach (var type in types)
        {
            var interfaceType = mainType.GetInterfaceType(type);
            if (interfaceType is null) continue;

            var genericArguments = interfaceType.GetGenericArguments();
            if (genericArguments.Length == 1) // IHandler<TRequest>
                return new(mainType, genericArguments[0], typeof(Void));

            return new(mainType, genericArguments[0], genericArguments[1]); // IHandler<TRequest, TResponse>
        }

        throw new InvalidOperationException(
            $"The handler {mainType} does not implement IHandler<TRequest,TResponse> or IHandler<TRequest>.");
    }

    private static Type? GetInterfaceType(this Type interfaceType, Type mainType)
        => interfaceType
            .GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == mainType);
}