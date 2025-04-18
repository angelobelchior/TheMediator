using System.Diagnostics.CodeAnalysis;
using TheMediator.Core.Executors;
using TheMediator.Core.Inspectors;
using TheMediator.Core.Models;
using TheMediator.Core.Registries;

namespace TheMediator.Core.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddTheMediator(
        this IServiceCollection services,
        Action<Configuration> configurator)
    {
        var configuration = new Configuration(services);
        configurator(configuration);

        services.AddSingleton<HandlerRegistry>(_ => configuration.Handlers);
        services.AddSingleton<FilterRegistry>(_ => configuration.Filters);
        //services.AddSingleton<NotificationRegistry>(_ => configuration.Notifications);

        services.AddSingleton<FilterExecutor>();
        //services.AddSingleton<NotificationExecutor>();
        services.AddSingleton<HandlerExecutor>();

        services.AddSingleton<ISender, Sender>();

        return services;
    }

    public class Configuration(IServiceCollection services)
    {
        internal HandlerRegistry Handlers { get; } = new(services);
        internal FilterRegistry Filters { get; } = new(services);
        //internal NotificationRegistry Notifications { get; } = new(services);

        public Configuration AddServicesFromAssemblies(params Assembly[] assemblies)
        {
            if (assemblies is null || assemblies.Length == 0)
                throw new ArgumentNullException(nameof(assemblies), "Assemblies cannot be null or empty.");

            assemblies
                .Select(ServiceInspector.GetServicesByAssembly)
                .SelectMany(serviceType => serviceType)
                .ToList()
                .ForEach(serviceDescriptor =>
                {
                    if (serviceDescriptor.Category == ServiceCategory.Handler)
                        Handlers.Add(serviceDescriptor);

                    //TODO: Incluir notificações...
                    // if (serviceDescriptor.Category == ServiceCategory.Notification)
                    //     Notifications.Add(serviceDescriptor);
                });

            return this;
        }

        public Configuration AddFilter<TFilter>()
            where TFilter : class, IRequestFilter
        {
            Filters.Add<TFilter>();
            return this;
        }
    }
}