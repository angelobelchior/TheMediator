using TheMediator.Core.Models;
using TheMediator.Core.Registries;

namespace TheMediator.Core.Tests.Registries;

public class HandlerRegistryTests
{
    [Fact]
    public void AddGenericRequestResponse_ShouldAddHandlerDescriptor()
    {
        // Arrange
        var registry = new HandlerRegistry();

        var handlerDescriptor = new HandlerDescriptor(
            typeof(RequestResponseRequestHandler),
            typeof(SampleRequest),
            typeof(SampleResponse));

        // Act
        registry.Add<RequestResponseRequestHandler>();

        // Assert
        var handler = registry.GetHandler<SampleRequest, SampleResponse>();
        Assert.Equal(handlerDescriptor, handler);
    }

    [Fact]
    public void AddGenericRequest_ShouldAddVoidHandlerDescriptor()
    {
        // Arrange
        var registry = new HandlerRegistry();

        var handlerDescriptor = new HandlerDescriptor(
            typeof(RequestRequestHandler),
            typeof(SampleRequest),
            null);

        // Act
        registry.Add<RequestRequestHandler>();

        // Assert
        var handler = registry.GetHandler<SampleRequest>();
        Assert.Equal(handlerDescriptor, handler);
    }

    [Fact]
    public void AddService_ShouldAddHandlerDescriptor()
    {
        // Arrange
        var registry = new HandlerRegistry();

        var handlerDescriptor = new HandlerDescriptor(typeof(string), typeof(int), typeof(bool));

        // Act
        registry.Add(handlerDescriptor);

        // Assert
        var handler = registry.GetHandler<int, bool>();
        Assert.Equal(handlerDescriptor, handler);
    }

    [Fact]
    public void Add_ShouldThrowInvalidOperationException_WhenDuplicateHandlerIsAdded()
    {
        // Arrange
        var registry = new HandlerRegistry();

        var handlerDescriptor = new HandlerDescriptor(typeof(string), typeof(int), typeof(bool));

        registry.Add(handlerDescriptor);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            registry.Add(handlerDescriptor));

        Assert.Contains("Cannot register", exception.Message);
    }

    [Fact]
    public void Add_ShouldExposeHandlerType()
    {
        // Arrange
        var registry = new HandlerRegistry();

        // Act
        registry.Add<RequestResponseRequestHandler>();

        // Assert
        Assert.Contains(typeof(RequestResponseRequestHandler), registry.ServiceTypes);
    }

    [Fact]
    public void GetHandler_ShouldReturnCorrectHandlerDescriptor()
    {
        // Arrange
        var registry = new HandlerRegistry();

        var handlerDescriptor = new HandlerDescriptor(typeof(string), typeof(int), typeof(bool));

        registry.Add(handlerDescriptor);

        // Act
        var handler = registry.GetHandler<int, bool>();

        // Assert
        Assert.NotNull(handler);
        Assert.Equal(handlerDescriptor, handler);
    }

    [Fact]
    public void GetHandler_ShouldThrowInvalidOperationException_WhenHandlerNotFound()
    {
        // Arrange
        var registry = new HandlerRegistry();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            registry.GetHandler<int, bool>());

        Assert.Contains("No Handler found", exception.Message);
    }
}
