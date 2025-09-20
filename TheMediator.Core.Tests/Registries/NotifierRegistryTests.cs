using TheMediator.Core.Registries;
using Microsoft.Extensions.DependencyInjection;
using TheMediator.Core.Models;

namespace TheMediator.Core.Tests.Registries;

public class NotifierRegistryTests
{
    private readonly IServiceCollection _services = new ServiceCollection();

    [Fact]
    public void Add_ShouldRegisterNotificationService()
    {
        // Arrange
        var registry = new NotifierRegistry(_services);

        // Act
        registry.Add<SampleNotification>();

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(SampleNotification));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddService_ShouldAddNotificationToList()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new NotifierRegistry(services);
        var notificationDescriptor = new Models.ServiceDescriptor(
            typeof(SampleNotification),
            typeof(SampleMessage),
            typeof(Models.Void),
            ServiceCategory.Notification);

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
        var registry = new NotifierRegistry(_services);

        // Act
        var notifiers = registry.ListNotifiersByMessageType<SampleMessage>();

        // Assert
        Assert.Empty(notifiers);
    }

    [Fact]
    public void Add_ShouldThrowException_WhenNotificationAlreadyRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new NotifierRegistry(services);
        var notificationDescriptor = new Models.ServiceDescriptor(
            typeof(SampleNotification),
            typeof(SampleMessage),
            typeof(Models.Void),
            ServiceCategory.Notification);

        registry.Add(notificationDescriptor);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => registry.Add(notificationDescriptor));
        Assert.Equal(
            $"Cannot register {notificationDescriptor.MainType}!.\n" +
            $"The Notification {notificationDescriptor.MainType} is already registered",
            exception.Message);
    }

    [Fact]
    public void Add_ShouldThrowException_WhenServiceDescriptorIsNotNotification()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new NotifierRegistry(services);
        var invalidDescriptor = new Models.ServiceDescriptor(
            typeof(SampleNotification),
            typeof(SampleMessage),
            typeof(Models.Void),
            ServiceCategory.Filter);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => registry.Add(invalidDescriptor));
        Assert.Equal($"{invalidDescriptor.MainType} is not a notification type!", exception.Message);
    }

    [Fact]
    public void ListNotifiersByMessageType_ShouldReturnRegisteredNotifiers()
    {
        // Arrange
        var registry = new NotifierRegistry(_services);
        registry.Add<SampleNotification>();

        // Act
        var notifiers = registry.ListNotifiersByMessageType<SampleMessage>();

        // Assert
        Assert.Single(notifiers);
        Assert.Equal(typeof(SampleNotification), notifiers.First().MainType);
    }
}