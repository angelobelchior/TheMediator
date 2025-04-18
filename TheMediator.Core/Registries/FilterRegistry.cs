using TheMediator.Core.Inspectors;
using TheMediator.Core.Models;

namespace TheMediator.Core.Registries;

public class FilterRegistry(IServiceCollection services)
{
    private readonly Stack<Models.ServiceDescriptor> _filters = new();

    public void Add<TFilter>()
    {
        var mainType = typeof(TFilter);
        var service = ServiceInspector.CreateService(mainType, ServiceCategory.Filter);
        Add(service);
    }

    internal void Add(Models.ServiceDescriptor serviceDescriptor)
    {
        _filters.Push(serviceDescriptor);
        services.AddSingleton(serviceDescriptor.MainType);
    }
    
    internal IEnumerable<Models.ServiceDescriptor> ListFilters()
        => _filters;
}