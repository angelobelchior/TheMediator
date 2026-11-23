using TheMediator.Core.Registries;
using TheMediator.Core.Models;

namespace TheMediator.Core.Tests.Registries;

public class NotifierRegistryTests
{
    [Fact]
    public void Add_ShouldExposeNotificationType()
    {
        // Arrange
        var registry = new NotifierRegistry();

        // Act
        registry.Add<SampleNotification>();

        // Assert
        Assert.Contains(typeof(SampleNotification), registry.ServiceTypes);
    }

    [Fact]
    public void AddService_ShouldAddNotificationToList()
    {
        // Arrange
        var registry = new NotifierRegistry();
        var notificationDescriptor = new NotificationDescriptor(typeof(SampleNotification), typeof(SampleMessage));

        // Act
        registry.Add(notificationDescriptor);

        // Assert
        var notifications = registry.ListNotifiersByMessageType<SampleMessage>().ToList();
        Assert.Single(notifications);
        Assert.Contains(notifications, n => n.MainType == typeof(SampleNotification));
    }

    [Fact]
    public void ListNotifiersByMessageType_ShouldReturnEmpty_WhenNoNotifiersRegistered()
    {
        // Arrange
        var registry = new NotifierRegistry();

        // Act
        var notifiers = registry.ListNotifiersByMessageType<SampleMessage>();

        // Assert
        Assert.Empty(notifiers);
    }

    [Fact]
    public void Add_ShouldThrowException_WhenNotificationAlreadyRegistered()
    {
        // Arrange
        var registry = new NotifierRegistry();
        var notificationDescriptor = new NotificationDescriptor(typeof(SampleNotification), typeof(SampleMessage));

        registry.Add(notificationDescriptor);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => registry.Add(notificationDescriptor));
        Assert.Equal(
            $"Cannot register {notificationDescriptor.MainType}!.\n" +
            $"The Notification {notificationDescriptor.MainType} is already registered",
            exception.Message);
    }

    [Fact]
    public void ListNotifiersByMessageType_ShouldReturnRegisteredNotifiers()
    {
        // Arrange
        var registry = new NotifierRegistry();
        registry.Add<SampleNotification>();

        // Act
        var notifiers = registry.ListNotifiersByMessageType<SampleMessage>();

        // Assert
        Assert.Single(notifiers);
        Assert.Equal(typeof(SampleNotification), notifiers.First().MainType);
    }
}
