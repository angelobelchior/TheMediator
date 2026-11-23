using TheMediator.Core.Inspectors;

namespace TheMediator.Core.Registries;

public class FilterRegistry : ServiceRegistry<Models.FilterDescriptor>
{
    public FilterRegistry()
        : base(ServiceInspector.CreateFilterDescriptor)
    {
    }

    private readonly Stack<Models.FilterDescriptor> _filters = new();

    protected override IEnumerable<Models.FilterDescriptor> Entries => _filters;

    protected override void Insert(Models.FilterDescriptor descriptor) => _filters.Push(descriptor);

    internal IEnumerable<Models.FilterDescriptor> ListFiltersWithRequest<TRequest>()
        => _filters.Where(f => f.RequestType is null || f.RequestType == typeof(TRequest));
}
