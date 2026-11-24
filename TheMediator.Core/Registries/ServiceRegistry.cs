namespace TheMediator.Core.Registries;

public abstract class ServiceRegistry<TDescriptor>
    where TDescriptor : Models.ServiceDescriptor
{
    internal void Add(TDescriptor descriptor)
    {
        EnsureUnique(descriptor);
        Insert(descriptor);
    }

    internal void RegisterServices(IServiceCollection services)
    {
        foreach (var entry in Entries)
            services.AddScoped(entry.MainType);
    }

    internal IEnumerable<Type> ServiceTypes => Entries.Select(entry => entry.MainType);

    protected virtual void EnsureUnique(TDescriptor descriptor)
    {
    }

    protected abstract void Insert(TDescriptor descriptor);

    protected abstract IEnumerable<TDescriptor> Entries { get; }
}
