using TheMediator.Core.Models;
using TheMediator.Core.Registries;

namespace TheMediator.Core.Tests.Registries;

public class FilterRegistryTests
{
    [Fact]
    public void AddGeneric_ShouldAddFilterToStack()
    {
        // Arrange
        var registry = new FilterRegistry();

        // Act
        registry.Add<SampleRequestFilter>();

        // Assert
        var filters = registry.ListFiltersWithRequest<SampleRequest>().ToList();
        Assert.Single(filters);
        Assert.Contains(filters, f => f.MainType == typeof(SampleRequestFilter));
    }

    [Fact]
    public void AddService_ShouldAddFilterToStack()
    {
        // Arrange
        var registry = new FilterRegistry();
        var filterDescriptor = new FilterDescriptor(typeof(SampleRequestFilter), null);

        // Act
        registry.Add(filterDescriptor);

        // Assert
        var filters = registry.ListFiltersWithRequest<SampleRequest>().ToList();
        Assert.Single(filters);
        Assert.Contains(filters, f => f.MainType == typeof(SampleRequestFilter));
    }

    [Fact]
    public void Add_ShouldExposeFilterType()
    {
        // Arrange
        var registry = new FilterRegistry();

        // Act
        registry.Add<SampleRequestFilter>();

        // Assert
        Assert.Contains(typeof(SampleRequestFilter), registry.ServiceTypes);
    }

    [Fact]
    public void ListFiltersWithRequest_ShouldReturnOnlyGenericAndMatchingFilters()
    {
        // Arrange
        var registry = new FilterRegistry();

        registry.Add<SampleRequestFilter>();
        registry.Add<SampleSpecificFilter, SampleRequest>();

        // Act
        var matchingFilters = registry.ListFiltersWithRequest<SampleRequest>().ToList();
        var unrelatedFilters = registry.ListFiltersWithRequest<SampleMessage>().ToList();

        // Assert
        Assert.Equal(2, matchingFilters.Count);
        Assert.Single(unrelatedFilters);
        Assert.Contains(unrelatedFilters, f => f.MainType == typeof(SampleRequestFilter));
    }
}
