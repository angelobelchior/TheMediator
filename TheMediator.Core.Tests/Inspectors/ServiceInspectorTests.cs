using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TheMediator.Core.Inspectors;
using TheMediator.Core.Models;

namespace TheMediator.Core.Tests.Inspectors;

[ExcludeFromCodeCoverage]
public class ServiceInspectorTests
{
    [Fact]
    public void GetServicesByAssembly_ShouldReturnValidServices_WhenAssemblyContainsValidTypes()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = ServiceInspector.GetServicesByAssembly(assembly).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, descriptor => Assert.NotNull(descriptor.MainType));
    }

    [Fact]
    public void GetServicesByAssembly_ShouldReturnEmpty_WhenAssemblyHasNoValidTypes()
    {
        // Arrange
        var assembly = typeof(string).Assembly;

        // Act
        var result = ServiceInspector.GetServicesByAssembly(assembly).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void CreateHandlerDescriptor_ShouldReturnRequestResponseDescriptor()
    {
        // Arrange
        var mainType = typeof(RequestResponseRequestHandler);

        // Act
        var result = ServiceInspector.CreateHandlerDescriptor(mainType);

        // Assert
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleRequest), result.RequestType);
        Assert.Equal(typeof(SampleResponse), result.ResponseType);
    }

    [Fact]
    public void CreateHandlerDescriptor_ShouldReturnVoidHandlerDescriptor()
    {
        // Arrange
        var mainType = typeof(RequestRequestHandler);

        // Act
        var result = ServiceInspector.CreateHandlerDescriptor(mainType);

        // Assert
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleRequest), result.RequestType);
        Assert.Null(result.ResponseType);
    }

    [Fact]
    public void CreateHandlerDescriptor_ShouldThrow_WhenTypeIsInvalid()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ServiceInspector.CreateHandlerDescriptor(typeof(InvalidType)));
    }

    [Fact]
    public void CreateFilterDescriptor_ShouldReturnGenericFilter()
    {
        // Arrange
        var mainType = typeof(SampleRequestFilter);

        // Act
        var result = ServiceInspector.CreateFilterDescriptor(mainType);

        // Assert
        Assert.Equal(mainType, result.MainType);
        Assert.Null(result.RequestType);
    }

    [Fact]
    public void CreateFilterDescriptor_ShouldReturnSpecificFilter()
    {
        // Arrange
        var mainType = typeof(SampleSpecificFilter);

        // Act
        var result = ServiceInspector.CreateFilterDescriptor(mainType);

        // Assert
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleRequest), result.RequestType);
    }

    [Fact]
    public void CreateFilterDescriptor_ShouldThrow_WhenTypeIsInvalid()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ServiceInspector.CreateFilterDescriptor(typeof(InvalidType)));
    }

    [Fact]
    public void CreateNotificationDescriptor_ShouldReturnNotificationDescriptor()
    {
        // Arrange
        var mainType = typeof(SampleNotification);

        // Act
        var result = ServiceInspector.CreateNotificationDescriptor(mainType);

        // Assert
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleMessage), result.MessageType);
    }

    [Fact]
    public void CreateNotificationDescriptor_ShouldThrow_WhenTypeIsInvalid()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ServiceInspector.CreateNotificationDescriptor(typeof(InvalidType)));
    }

    [Fact]
    public void GetServiceDescriptorsForType_ShouldReturnHandlerDescriptor_WhenTypeImplementsHandlerInterface()
    {
        // Arrange
        var type = typeof(RequestResponseRequestHandler);

        // Act
        var descriptors = ServiceInspector.GetServiceDescriptorsForType(type).ToList();

        // Assert
        Assert.Single(descriptors);
        var descriptor = Assert.IsType<HandlerDescriptor>(descriptors.First());
        Assert.Equal(type, descriptor.MainType);
    }

    [Fact]
    public void GetServiceDescriptorsForType_ShouldReturnFilterDescriptor_WhenTypeImplementsFilterInterface()
    {
        // Arrange
        var type = typeof(SampleRequestFilter);

        // Act
        var descriptors = ServiceInspector.GetServiceDescriptorsForType(type).ToList();

        // Assert
        Assert.Single(descriptors);
        var descriptor = Assert.IsType<FilterDescriptor>(descriptors.First());
        Assert.Equal(type, descriptor.MainType);
    }

    [Fact]
    public void GetServiceDescriptorsForType_ShouldReturnEmpty_WhenTypeDoesNotImplementAnyInterface()
    {
        // Act
        var descriptors = ServiceInspector.GetServiceDescriptorsForType(typeof(UnrelatedClass));

        // Assert
        Assert.Empty(descriptors);
    }

    [Fact]
    public void GetMatchingInterface_ShouldReturnMatchingGenericInterface_WhenImplemented()
    {
        // Arrange
        var mainType = typeof(GenericHandler);
        var interfaceType = typeof(IRequestHandler<,>);

        // Act
        var result = ServiceInspector.GetMatchingInterface(mainType, interfaceType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(interfaceType, result.GetGenericTypeDefinition());
    }

    [Fact]
    public void GetMatchingInterface_ShouldReturnMatchingNonGenericInterface_WhenImplemented()
    {
        // Arrange
        var mainType = typeof(NonGenericHandler);
        var interfaceType = typeof(IRequestFilter);

        // Act
        var result = ServiceInspector.GetMatchingInterface(mainType, interfaceType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(interfaceType, result);
    }

    [Fact]
    public void GetMatchingInterface_ShouldReturnNull_WhenInterfaceNotImplemented()
    {
        // Arrange
        var mainType = typeof(UnrelatedClass);
        var interfaceType = typeof(IRequestHandler<,>);

        // Act
        var result = ServiceInspector.GetMatchingInterface(mainType, interfaceType);

        // Assert
        Assert.Null(result);
    }
}
