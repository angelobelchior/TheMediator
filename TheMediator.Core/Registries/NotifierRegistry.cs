using TheMediator.Core.Inspectors;

namespace TheMediator.Core.Registries;

/// <summary>
/// Notifier registry.
/// </summary>
/// <param name="services">Service Collection</param>
public class NotifierRegistry(IServiceCollection services) 
{
    private readonly List<Models.ServiceDescriptor> _notifications = new();

    /// <summary>
    /// Configuration for the notifier registry. <see cref="NotificationConfiguration"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public NotificationConfiguration Configurations { get; set; } = NotificationConfiguration.None;

    
    /// <summary>
    /// Adds a notifier to the registry.
    /// </summary>
    /// <typeparam name="TService">Service Collection</typeparam>
    public void Add<TService>()
        where TService : notnull
    {
        var mainType = typeof(TService);
        var service = ServiceInspector.CreateService(mainType, ServiceCategory.Notification);
        Add(service);
    }

    internal void Add(Models.ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.Category != ServiceCategory.Notification)
            throw new InvalidOperationException($"{serviceDescriptor.MainType} is not a notification type!");

        var current = _notifications.FirstOrDefault(s =>
            s.MainType == serviceDescriptor.MainType &&
            s.RequestType == serviceDescriptor.RequestType &&
            s.ResponseType == Models.Void.Type &&
            s.Category == serviceDescriptor.Category);
        if (current is not null)
            throw new InvalidOperationException(
                $"Cannot register {serviceDescriptor.MainType}!.\n" +
                $"The Notification {current.MainType} is already registered");

        _notifications.Add(serviceDescriptor);
        services.AddSingleton(serviceDescriptor.MainType);
    }

    internal IReadOnlyCollection<Models.ServiceDescriptor> ListNotifiersByMessageType<TMessageType>()
        => _notifications.Where(s => s.RequestType == typeof(TMessageType) &&
                                     s.ResponseType == Models.Void.Type &&
                                     s.Category == ServiceCategory.Notification).ToList();
}