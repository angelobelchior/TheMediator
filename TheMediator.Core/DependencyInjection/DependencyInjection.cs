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
        var configuration = new Configuration(services);
        configurator(configuration);

        services.AddSingleton<HandlerRegistry>(_ => configuration.Handlers);
        services.AddSingleton<FilterRegistry>(_ => configuration.Filters);
        services.AddSingleton<NotifierRegistry>(_ => configuration.Notifiers);

        services.AddSingleton<FilterExecutor>();
        services.AddSingleton<NotifierExecutor>();
        services.AddSingleton<HandlerExecutor>();

        services.AddSingleton<ISender, Sender>();
        services.AddSingleton<IPublisher, Publisher>();

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
        public HandlerRegistry Handlers { get; }
        /// <summary>
        /// Filter registry for TheMediator.
        /// </summary>
        public FilterRegistry Filters { get; }
        /// <summary>
        /// Notifier registry for TheMediator.
        /// </summary>
        public NotifierRegistry Notifiers { get; }

        internal Configuration(IServiceCollection services)
        {
            Handlers = new(services);
            Filters = new(services);
            Notifiers = new(services);
        }

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
                    if (serviceDescriptor.Category == ServiceCategory.Handler)
                        Handlers.Add(serviceDescriptor);
                    if (serviceDescriptor.Category == ServiceCategory.Notification)
                        Notifiers.Add(serviceDescriptor);
                });

            return this;
        }
    }
}