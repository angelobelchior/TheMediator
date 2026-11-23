namespace TheMediator.Core.Registries;

public abstract class ServiceRegistry<TDescriptor>(Func<Type, TDescriptor> descriptorFactory)
    where TDescriptor : Models.ServiceDescriptor
{
    public void Add<TService>()
        where TService : notnull
        => Add(descriptorFactory(typeof(TService)));

    internal void Add(TDescriptor descriptor)
    {
        EnsureUnique(descriptor);
        Insert(descriptor);
    }

    internal IEnumerable<Type> ServiceTypes => Entries.Select(entry => entry.MainType);

    protected virtual void EnsureUnique(TDescriptor descriptor)
    {
    }

    protected abstract void Insert(TDescriptor descriptor);

    protected abstract IEnumerable<TDescriptor> Entries { get; }
}
