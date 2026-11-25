using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheMediator.Core.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class TheMediatorSourceGenerator : IIncrementalGenerator
{
    private const string CoreNamespace = "TheMediator.Core";
    private const string AttributeFullName = "TheMediator.Core.TheMediatorImplementationAttribute";
    private const string MethodName = "RegisterFromAssembly";

    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static readonly SymbolDisplayFormat TypeOfFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var localTypes = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
            .Where(static symbol => symbol is not null)
            .Collect();

        var registerTargets = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is GenericNameSyntax { Identifier.ValueText: MethodName },
                static (ctx, _) => GetTargetType(ctx))
            .Where(static type => type is not null)
            .Collect();

        var combined = context.CompilationProvider.Combine(localTypes).Combine(registerTargets);
        context.RegisterSourceOutput(combined, static (spc, source) =>
            Execute(spc, source.Left.Left, source.Left.Right, source.Right));
    }

    private static ITypeSymbol? GetTargetType(GeneratorSyntaxContext ctx)
    {
        var genericName = (GenericNameSyntax)ctx.Node;
        var arguments = genericName.TypeArgumentList.Arguments;
        if (arguments.Count != 1)
            return null;
        return ctx.SemanticModel.GetTypeInfo(arguments[0]).Type;
    }

    private static void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol?> localTypes,
        ImmutableArray<ITypeSymbol?> registerTargets)
    {
        var localRegistrations = new List<Registration>();
        foreach (var type in localTypes)
        {
            var registration = TryCreateRegistration(type);
            if (registration is not null)
                localRegistrations.Add(registration);
        }

        if (localRegistrations.Count > 0)
            context.AddSource("TheMediator.Generated.Attributes.g.cs", BuildAssemblyAttribute(localRegistrations));

        var groups = new List<AssemblyGroup>();
        var seen = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default);
        foreach (var target in registerTargets)
        {
            var assembly = target?.ContainingAssembly;
            if (assembly is null || !seen.Add(assembly))
                continue;

            var registrations = SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly)
                ? localRegistrations
                : ReadAssemblyRegistrations(assembly);

            groups.Add(new AssemblyGroup(target!, registrations));
        }

        if (groups.Count > 0)
            context.AddSource("TheMediator.Generated.g.cs", BuildSource(groups));
    }

    private static Registration? TryCreateRegistration(INamedTypeSymbol? type)
    {
        if (type is null || type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsGenericType)
            return null;

        if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
            return null;

        foreach (var iface in type.AllInterfaces)
        {
            if (IsInterface(iface, "IRequestHandler", 2))
                return new Registration(type, iface.TypeArguments[0], iface.TypeArguments[1],
                    RegistrationKind.Handler);
            if (IsInterface(iface, "IRequestHandler", 1))
                return new Registration(type, iface.TypeArguments[0], null, RegistrationKind.Handler);
            if (IsInterface(iface, "INotificationHandler", 1))
                return new Registration(type, iface.TypeArguments[0], null, RegistrationKind.Notifier);
        }

        return null;
    }

    private static List<Registration> ReadAssemblyRegistrations(IAssemblySymbol assembly)
    {
        var registrations = new List<Registration>();
        foreach (var attribute in assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != AttributeFullName)
                continue;
            if (attribute.ConstructorArguments.Length != 1)
                continue;

            var argument = attribute.ConstructorArguments[0];
            if (argument.Kind != TypedConstantKind.Array)
                continue;

            foreach (var element in argument.Values)
            {
                if (element.Kind == TypedConstantKind.Type && element.Value is INamedTypeSymbol type)
                {
                    var registration = TryCreateRegistration(type);
                    if (registration is not null)
                        registrations.Add(registration);
                }
            }
        }

        return registrations;
    }

    private static bool IsInterface(INamedTypeSymbol namedTypeSymbol, string name, int arity)
        => namedTypeSymbol.OriginalDefinition.Name == name
           && namedTypeSymbol.OriginalDefinition.Arity == arity
           && namedTypeSymbol.OriginalDefinition.ContainingNamespace?.ToDisplayString() == CoreNamespace;

    private static string BuildAssemblyAttribute(List<Registration> registrations)
    {
        var types = string.Join(", ",
            registrations.Select(r => $"typeof({r.ServiceType.ToDisplayString(TypeOfFormat)})"));

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine($"[assembly: global::{AttributeFullName}({types})]");
        return sb.ToString();
    }

    private static string BuildSource(List<AssemblyGroup> groups)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace TheMediator.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    internal static class TheMediatorDI");
        sb.AppendLine("    {");
        sb.AppendLine("        public static global::TheMediator.Core.DependencyInjection.DependencyInjection.Configuration RegisterFromAssembly<T>(");
        sb.AppendLine("            this global::TheMediator.Core.DependencyInjection.DependencyInjection.Configuration configuration)");
        sb.AppendLine("        {");
        foreach (var group in groups)
        {
            sb.AppendLine($"            if (typeof(T).Assembly == typeof({group.Representative.ToDisplayString(TypeOfFormat)}).Assembly)");
            sb.AppendLine("            {");
            foreach (var registration in group.Registrations)
                sb.AppendLine(BuildRegistrationLine(registration, "                "));
            sb.AppendLine("            }");
        }
        sb.AppendLine("            return configuration;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildRegistrationLine(Registration registration, string indent)
    {
        var serviceType = registration.ServiceType.ToDisplayString(TypeFormat);
        return registration.Kind switch
        {
            RegistrationKind.Notifier =>
                $"{indent}configuration.Notifiers.Add<{serviceType}, {registration.FirstType!.ToDisplayString(TypeFormat)}>();",
            _ => registration.SecondType is not null
                ? $"{indent}configuration.Handlers.Add<{serviceType}, {registration.FirstType!.ToDisplayString(TypeFormat)}, {registration.SecondType.ToDisplayString(TypeFormat)}>();"
                : $"{indent}configuration.Handlers.Add<{serviceType}, {registration.FirstType!.ToDisplayString(TypeFormat)}>();"
        };
    }

    private enum RegistrationKind
    {
        Handler,
        Notifier
    }

    private sealed record Registration(
        INamedTypeSymbol ServiceType,
        ITypeSymbol? FirstType,
        ITypeSymbol? SecondType,
        RegistrationKind Kind);

    private sealed record AssemblyGroup(
        ITypeSymbol Representative,
        List<Registration> Registrations);
}
