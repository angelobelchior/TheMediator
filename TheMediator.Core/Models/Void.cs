namespace TheMediator.Core.Models;

[ExcludeFromCodeCoverage]
internal class Void
{
    public static Void Null { get; } = new();

    public static Type Type { get; } = typeof(Void);
}