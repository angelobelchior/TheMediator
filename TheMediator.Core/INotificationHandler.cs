namespace TheMediator.Core;

/// <summary>
/// Notification handler interface.
/// </summary>
/// <typeparam name="TNotification">Notification type.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : notnull
{
    /// <summary>
    /// Handle the notification asynchronously.
    /// </summary>
    /// <param name="notification">Notification</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns></returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
}