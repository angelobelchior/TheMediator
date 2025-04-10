namespace TheMediator.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddTheMediator(
        this IServiceCollection services, Action<HandlerTypeManager> configurator)
    {
        var handlerTypeManager = new HandlerTypeManager(services);
        configurator(handlerTypeManager);

        services.AddSingleton<HandlerTypeManager>(_ => handlerTypeManager);
        services.AddSingleton<ISender, Sender>();

        return services;
    }
}