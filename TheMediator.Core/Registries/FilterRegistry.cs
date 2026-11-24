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

    internal IEnumerable<FilterDescriptor> ListFiltersWithRequest<TRequest>()
        => _filters.Where(f => f.RequestType is null || f.RequestType == typeof(TRequest));
}
