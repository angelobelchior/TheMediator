using TheMediator.Core.Executors;
using TheMediator.Core.Inspectors;
using TheMediator.Core.Registries;

namespace TheMediator.Core.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for TheMediator.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    /// <summary>
    /// Adds TheMediator services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configurator">Configuration action to set up TheMediator services.</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddTheMediator(
        this IServiceCollection services,
        Action<Configuration> configurator)
    {
        var configuration = new Configuration();
        configurator(configuration);

        foreach (var serviceType in configuration.Handlers.ServiceTypes
                     .Concat(configuration.Filters.ServiceTypes)
                     .Concat(configuration.Notifiers.ServiceTypes))
            services.AddScoped(serviceType);

        services.AddSingleton(configuration.Handlers);
        services.AddSingleton(configuration.Filters);
        services.AddSingleton(configuration.Notifiers);

        services.AddScoped<FilterExecutor>();
        services.AddScoped<NotifierExecutor>();
        services.AddScoped<HandlerExecutor>();

        services.AddScoped<ISender, Sender>();
        services.AddScoped<IPublisher, Publisher>();

        return services;
    }

    /// <summary>
    /// Configuration class for TheMediator services.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Handler registry for TheMediator.
        /// </summary>
        public HandlerRegistry Handlers { get; } = new();

        /// <summary>
        /// Filter registry for TheMediator.
        /// </summary>
        public FilterRegistry Filters { get; } = new();

        /// <summary>
        /// Notifier registry for TheMediator.
        /// </summary>
        public NotifierRegistry Notifiers { get; } = new();

        /// <summary>
        /// Adds services from the specified assemblies to TheMediator.
        /// </summary>
        /// <param name="assemblies">Assemblies</param>
        /// <returns>Configuration</returns>
        /// <exception cref="ArgumentNullException">When assemblies is null or empty</exception>
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
                    switch (serviceDescriptor)
                    {
                        case HandlerDescriptor handler:
                            Handlers.Add(handler);
                            break;
                        case NotificationDescriptor notification:
                            Notifiers.Add(notification);
                            break;
                    }
                });

            return this;
        }
    }
}
