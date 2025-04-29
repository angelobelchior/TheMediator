using TheMediator.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using TheMediator.Core.Registries;

namespace TheMediator.Core.Tests.Registries;

public class HandlerRegistryTests
{
    [Fact]
    public void AddGenericRequestResponse_ShouldAddServiceDescriptorToHandlers()
    {
        // Arrange
        var registry = new HandlerRegistry(new ServiceCollection());

        var serviceDescriptor = new Models.ServiceDescriptor(
            typeof(RequestResponseRequestHandler),
            typeof(SampleRequest),
            typeof(SampleResponse),
            ServiceCategory.Handler);


        // Act
        registry.Add<RequestResponseRequestHandler>();

        // Assert
        var handler = registry.GetHandler<SampleRequest, SampleResponse>();
        Assert.Equal(serviceDescriptor, handler);
    }

    [Fact]
    public void AddGenericRequest_ShouldAddServiceDescriptorToHandlers()
    {
        // Arrange
        var registry = new HandlerRegistry(new ServiceCollection());

        var serviceDescriptor = new Models.ServiceDescriptor(
            typeof(RequestRequestHandler),
            typeof(SampleRequest),
            typeof(Models.Void),
            ServiceCategory.Handler);

        // Act
        registry.Add<RequestRequestHandler>();

        // Assert
        var handler = registry.GetHandler<SampleRequest, Models.Void>();
        Assert.Equal(serviceDescriptor, handler);
    }

    [Fact]
    public void AddService_ShouldAddServiceDescriptorToHandlers()
    {
        // Arrange
        var registry = new HandlerRegistry(new ServiceCollection());

        // Arrange
        var serviceDescriptor = new Models.ServiceDescriptor(
            typeof(string), typeof(int), typeof(bool), ServiceCategory.Handler);

        // Act
        registry.Add(serviceDescriptor);

        // Assert
        var handler = registry.GetHandler<int, bool>();
        Assert.Equal(serviceDescriptor, handler);
    }

    [Fact]
    public void Add_ShouldThrowInvalidOperationException_WhenDuplicateHandlerIsAdded()
    {
        // Arrange
        var registry = new HandlerRegistry(new ServiceCollection());

        // Arrange
        var serviceDescriptor = new Models.ServiceDescriptor(
            typeof(string), typeof(int), typeof(bool), ServiceCategory.Handler);

        registry.Add(serviceDescriptor);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            registry.Add(serviceDescriptor));

        Assert.Contains("Cannot register", exception.Message);
    }

    [Fact]
    public void Add_ShouldRegisterHandlerInServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new HandlerRegistry(services);

        // Act
        registry.Add<RequestResponseRequestHandler>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService(typeof(RequestResponseRequestHandler));
        Assert.NotNull(handler);
    }

    [Fact]
    public void GetHandler_ShouldReturnCorrectServiceDescriptor()
    {
        // Arrange
        var registry = new HandlerRegistry(new ServiceCollection());

        // Arrange
        var serviceDescriptor = new Models.ServiceDescriptor(
            typeof(string), typeof(int), typeof(bool), ServiceCategory.Handler);

        registry.Add(serviceDescriptor);

        // Act
        var handler = registry.GetHandler<int, bool>();

        // Assert
        Assert.NotNull(handler);
        Assert.Equal(serviceDescriptor, handler);
    }

    [Fact]
    public void GetHandler_ShouldThrowInvalidOperationException_WhenHandlerNotFound()
    {
        // Arrange
        var registry = new HandlerRegistry(new ServiceCollection());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            registry.GetHandler<int, bool>());

        Assert.Contains("No Handler found", exception.Message);
    }
}