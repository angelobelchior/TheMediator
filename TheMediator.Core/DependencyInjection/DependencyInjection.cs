using TheMediator.Core.Executors;
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

        configuration.Handlers.RegisterServices(services);
        configuration.Filters.RegisterServices(services);
        configuration.Notifiers.RegisterServices(services);

        services.AddSingleton(configuration.Handlers);
        services.AddSingleton(configuration.Filters);
        services.AddSingleton(configuration.Notifiers);

        services.AddScoped<FilterExecutor>();
        services.AddScoped<ISender, HandlerExecutor>();
        services.AddScoped<IPublisher, NotifierExecutor>();

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
    }
}
