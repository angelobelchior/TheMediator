using TheMediator.Core.Inspectors;

namespace TheMediator.Core.Registries;

/// <summary>
/// Filter registry.
/// </summary>
/// <param name="services">Service Collection</param>
public class FilterRegistry(IServiceCollection services)
{
    private readonly Stack<Models.ServiceDescriptor> _filters = new();

    /// <summary>
    /// Add a filter to the registry.
    /// </summary>
    /// <typeparam name="TFilter">Filter Type</typeparam>
    public void Add<TFilter>()
    {
        var mainType = typeof(TFilter);
        var service = ServiceInspector.CreateService(mainType, ServiceCategory.Filter);
        Add(service);
    }

    internal void Add(Models.ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.Category != ServiceCategory.Filter)
            throw new InvalidOperationException($"{serviceDescriptor.MainType} is not a filter type!");

        _filters.Push(serviceDescriptor);
        services.AddScoped(serviceDescriptor.MainType);
    }

    internal IEnumerable<Models.ServiceDescriptor> ListFiltersWithRequest<TRequest>()
        => _filters.Where(f => (f.RequestType == typeof(TRequest) &&
                                f.ResponseType == Models.Void.Type) || (f.RequestType == Models.Void.Type &&
                                                                        f.ResponseType == Models.Void.Type));
}