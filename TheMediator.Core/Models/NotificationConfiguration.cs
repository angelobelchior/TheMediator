namespace TheMediator.Core.Models;

/// <summary>
/// Notification configuration.
/// </summary>
/// <param name="deliveryMode"></param>
[ExcludeFromCodeCoverage]
public class NotificationConfiguration(NotificationDeliveryMode deliveryMode)
{
    public NotificationDeliveryMode DeliveryMode { get; set; } = deliveryMode;

    public static readonly NotificationConfiguration None = new(NotificationDeliveryMode.FireAndForget);
}

/// <summary>
/// Notification delivery mode.
/// </summary>
public enum NotificationDeliveryMode
{
    /// <summary>
    /// Fire and forget mode. The notification is sent, and the sender does not wait for a response.
    /// </summary>
    FireAndForget,
    
    /// <summary>
    /// Wait for all handlers to complete. The sender waits for all handlers to complete before continuing.
    /// </summary>
    WaitAll
}