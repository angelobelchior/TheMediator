using TheMediator.Core.Models;

namespace TheMediator.Core.Tests.Registries;

using Microsoft.Extensions.DependencyInjection;
using TheMediator.Core.Registries;
using Xunit;

public class FilterRegistryTests
{
    [Fact]
    public void AddGeneric_ShouldAddFilterToStack()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new FilterRegistry(services);

        // Act
        registry.Add<SampleRequestFilter>();

        // Assert
        var filters = registry.ListFilters().ToList();
        Assert.Single(filters);
        Assert.Contains(filters, f => f.MainType == typeof(SampleRequestFilter));
    }
    
    [Fact]
    public void AddService_ShouldAddFilterToStack()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new FilterRegistry(services);
        var filterDescriptor = new Models.ServiceDescriptor(
            typeof(SampleRequestFilter),
            typeof(object),
            typeof(object),
            ServiceCategory.Filter);

        // Act
        registry.Add(filterDescriptor);

        // Assert
        var filters = registry.ListFilters().ToList();
        Assert.Single(filters);
        Assert.Contains(filters, f => f.MainType == typeof(SampleRequestFilter));
    }

    [Fact]
    public void Add_ShouldRegisterFilterInServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = new FilterRegistry(services);

        // Act
        registry.Add<SampleRequestFilter>();
        
        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var filter = serviceProvider.GetService(typeof(SampleRequestFilter));
        Assert.NotNull(filter);
    }
}