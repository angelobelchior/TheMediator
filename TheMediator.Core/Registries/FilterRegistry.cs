namespace TheMediator.Core.Registries;

public class FilterRegistry : ServiceRegistry<FilterDescriptor>
{
    public void Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilter,
        TRequest>()
        where TFilter : IRequestFilter<TRequest>
        where TRequest : notnull
        => Add(new FilterDescriptor(typeof(TFilter), typeof(TRequest)));

    public void Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilter>()
        where TFilter : IRequestFilter
        => Add(new FilterDescriptor(typeof(TFilter), null));

    private readonly Stack<FilterDescriptor> _filters = new();

    protected override IEnumerable<FilterDescriptor> Entries => _filters;

    protected override void Insert(FilterDescriptor descriptor) => _filters.Push(descriptor);

    internal IReadOnlyList<FilterDescriptor> ListFiltersWithRequest<TRequest>()
    {
        var type = typeof(TRequest);
        List<FilterDescriptor>? result = null;

        foreach (var filter in _filters)
        {
            if (filter.RequestType is null || filter.RequestType == type)
            {
                result ??= new List<FilterDescriptor>();
                result.Add(filter);
            }
        }

        return result is null
            ? Array.Empty<FilterDescriptor>()
            : result;
    }
}
