namespace TheMediator.Core;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class TheMediatorImplementationAttribute : Attribute
{
    public TheMediatorImplementationAttribute(params Type[] types) => Types = types;

    public Type[] Types { get; }
}
