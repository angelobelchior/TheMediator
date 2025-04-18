using System.Diagnostics.CodeAnalysis;

namespace TheMediator.Core.Tests.Inspectors;

using System.Reflection;
using TheMediator.Core.Inspectors;
using Models;
using Xunit;

[ExcludeFromCodeCoverage]
public class ServiceInspectorTests
{
    [Fact]
    public void CreateService_ShouldReturnValidServiceDescriptor_ForHandlerRequestResponse()
    {
        // Arrange
        var mainType = typeof(RequestResponseRequestHandler);
        var serviceCategory = ServiceCategory.Handler;

        // Act
        var result = ServiceInspector.CreateService(mainType, serviceCategory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleRequest), result.RequestType);
        Assert.Equal(typeof(SampleResponse), result.ResponseType);
        Assert.Equal(serviceCategory, result.Category);
    }

    [Fact]
    public void CreateService_ShouldReturnValidServiceDescriptor_ForHandlerRequest()
    {
        // Arrange
        var mainType = typeof(RequestRequestHandler);
        var serviceCategory = ServiceCategory.Handler;

        // Act
        var result = ServiceInspector.CreateService(mainType, serviceCategory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleRequest), result.RequestType);
        Assert.Equal(typeof(Void), result.ResponseType);
        Assert.Equal(serviceCategory, result.Category);
    }

    [Fact]
    public void CreateServiceFromInterface_ShouldReturnDescriptor_WithNoGenericArguments()
    {
        // Arrange
        var mainType = typeof(SampleRequestFilter);
        var interfaceType = typeof(IRequestFilter);
        var serviceCategory = ServiceCategory.Filter;
        var voidType = typeof(Void);

        // Act
        var result = ServiceInspector.CreateServiceFromInterface(mainType, interfaceType, serviceCategory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(voidType, result.RequestType);
        Assert.Equal(voidType, result.ResponseType);
        Assert.Equal(serviceCategory, result.Category);
    }

    [Fact]
    public void CreateServiceFromInterface_ShouldReturnDescriptor_WithOneGenericArgument()
    {
        // Arrange
        var mainType = typeof(RequestRequestHandler);
        var interfaceType = typeof(IRequestHandler<SampleRequest>);
        var serviceCategory = ServiceCategory.Handler;
        var voidType = typeof(Void);

        // Act
        var result = ServiceInspector.CreateServiceFromInterface(mainType, interfaceType, serviceCategory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleRequest), result.RequestType);
        Assert.Equal(voidType, result.ResponseType);
        Assert.Equal(serviceCategory, result.Category);
    }

    [Fact]
    public void CreateServiceFromInterface_ShouldReturnDescriptor_WithTwoGenericArguments()
    {
        // Arrange
        var mainType = typeof(RequestResponseRequestHandler);
        var interfaceType = typeof(IRequestHandler<SampleRequest, SampleResponse>);
        var serviceCategory = ServiceCategory.Handler;
        var voidType = typeof(Void);

        // Act
        var result = ServiceInspector.CreateServiceFromInterface(mainType, interfaceType, serviceCategory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(SampleRequest), result.RequestType);
        Assert.Equal(typeof(SampleResponse), result.ResponseType);
        Assert.Equal(serviceCategory, result.Category);
    }

    [Fact]
    public void CreateService_ShouldReturnValidServiceDescriptor_ForFilter()
    {
        // Arrange
        var mainType = typeof(SampleRequestFilter);
        var serviceCategory = ServiceCategory.Filter;

        // Act
        var result = ServiceInspector.CreateService(mainType, serviceCategory);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mainType, result.MainType);
        Assert.Equal(typeof(Void), result.RequestType);
        Assert.Equal(typeof(Void), result.ResponseType);
        Assert.Equal(serviceCategory, result.Category);
    }

    [Fact]
    public void CreateService_ShouldThrowException_WhenTypeIsInvalid()
    {
        // Arrange
        var mainType = typeof(InvalidType);
        var serviceCategory = ServiceCategory.Handler;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ServiceInspector.CreateService(mainType, serviceCategory));
        Assert.Contains("does not implement a valid handler or filter handler interface", exception.Message);
    }

    [Fact]
    public void GetServicesByAssembly_ShouldReturnValidServices()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = ServiceInspector.GetServicesByAssembly(assembly).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, descriptor =>
        {
            var (mainType, _, _, _) = descriptor;
            Assert.NotNull(mainType);
        });
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
