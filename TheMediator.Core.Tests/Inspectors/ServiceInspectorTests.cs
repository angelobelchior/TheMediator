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
    public void GetServicesByAssembly_ShouldReturnValidServices_WhenAssemblyContainsValidTypes()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = ServiceInspector.GetServicesByAssembly(assembly).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, descriptor =>
        {
            var (mainType, requestType, responseType, serviceCategory) = descriptor;
            Assert.NotNull(mainType);
            Assert.NotNull(requestType);
            Assert.NotNull(responseType);
            Assert.IsType<ServiceCategory>(serviceCategory);
        });
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
        Assert.Throws<InvalidOperationException>(() =>
            ServiceInspector.CreateService(mainType, serviceCategory));
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
        var descriptor = descriptors.First();
        Assert.Equal(typeof(RequestResponseRequestHandler), descriptor.MainType);
        Assert.Equal(ServiceCategory.Handler, descriptor.Category);
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
        var descriptor = descriptors.First();
        Assert.Equal(typeof(SampleRequestFilter), descriptor.MainType);
        Assert.Equal(ServiceCategory.Filter, descriptor.Category);
    }

    [Fact]
    public void GetServiceDescriptorsForType_ShouldReturnEmpty_WhenTypeDoesNotImplementAnyInterface()
    {
        // Arrange
        var type = typeof(UnrelatedClass);

        // Act
        var descriptors = ServiceInspector.GetServiceDescriptorsForType(type);

        // Assert
        Assert.Empty(descriptors);
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