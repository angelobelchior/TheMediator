using TheMediator.Core.Inspectors;

namespace TheMediator.Core.Registries;

public class NotifierRegistry : ServiceRegistry<Models.NotificationDescriptor>
{
    public NotifierRegistry()
        : base(ServiceInspector.CreateNotificationDescriptor)
    {
    }

    private readonly List<Models.NotificationDescriptor> _notifications = new();

    [ExcludeFromCodeCoverage]
    public NotificationConfiguration Configurations { get; set; } = NotificationConfiguration.None;

    protected override IEnumerable<Models.NotificationDescriptor> Entries => _notifications;

    protected override void EnsureUnique(Models.NotificationDescriptor descriptor)
    {
        var current = _notifications.FirstOrDefault(s =>
            s.MainType == descriptor.MainType &&
            s.MessageType == descriptor.MessageType);
        if (current is not null)
            throw new InvalidOperationException(
                $"Cannot register {descriptor.MainType}!.\n" +
                $"The Notification {current.MainType} is already registered");
    }

    protected override void Insert(Models.NotificationDescriptor descriptor) => _notifications.Add(descriptor);

    internal IReadOnlyCollection<Models.NotificationDescriptor> ListNotifiersByMessageType<TMessageType>()
        => _notifications.Where(s => s.MessageType == typeof(TMessageType)).ToList();
}
