namespace TheMediator.Core.Registries;

public class NotifierRegistry : ServiceRegistry<NotificationDescriptor>
{
    public void Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TNotifier,
        TMessage>()
        where TNotifier : INotificationHandler<TMessage>
        where TMessage : notnull
        => Add(new NotificationDescriptor(typeof(TNotifier), typeof(TMessage)));

    private readonly List<NotificationDescriptor> _notifications = new();

    [ExcludeFromCodeCoverage]
    public NotificationSettings Settings { get; set; } = NotificationSettings.None;

    protected override IEnumerable<NotificationDescriptor> Entries => _notifications;

    protected override void EnsureUnique(NotificationDescriptor descriptor)
    {
        var current = _notifications.FirstOrDefault(s =>
            s.MainType == descriptor.MainType &&
            s.MessageType == descriptor.MessageType);
        if (current is not null)
            throw new InvalidOperationException(
                $"Cannot register {descriptor.MainType}!.\n" +
                $"The Notification {current.MainType} is already registered");
    }

    protected override void Insert(NotificationDescriptor descriptor) => _notifications.Add(descriptor);

    internal IReadOnlyCollection<NotificationDescriptor> ListNotifiersByMessageType<TMessageType>()
        => _notifications.Where(s => s.MessageType == typeof(TMessageType)).ToList();
}
