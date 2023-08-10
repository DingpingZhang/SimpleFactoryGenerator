using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SimpleFactoryGenerator.SourceGenerator;

internal static class DiagnosticDescriptors
{
    private const string SimpleFactoryGenerator = nameof(SimpleFactoryGenerator);

    public static readonly DiagnosticDescriptor NoGenericParameters = new(
        "SFG001",
        "The target interface or class must not have generic parameters",
        "Interfaces or classes with generic parameters are not supported, consider removing the generic parameters",
        SimpleFactoryGenerator,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor TheSameKeyType = new(
        "SFG002",
        "Classes with the same target interface must have the same key type",
        "If the target interfaces are the same, their key types must also be the same, consider using the same type of key",
        SimpleFactoryGenerator,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor ImplementTargetInterface = new(
        "SFG003",
        "The class must implement the target interface declared by the attribute",
        "The product class must implement the target interface declared on the attribute, consider implementing the interface '{0}' for class '{1}'",
        SimpleFactoryGenerator,
        DiagnosticSeverity.Error,
        true);

    public static bool CheckImplementTargetInterface(this GeneratorExecutionContext context, ITypeSymbol target, IEnumerable<INamedTypeSymbol> symbols)
    {
        var invalidClasses = symbols
            .Where(item => target.TypeKind is TypeKind.Interface
                // Inherited from interface
                ? !item.AllInterfaces.Any(@interface => @interface.Equals(target, SymbolEqualityComparer.Default))
                // Inherited from class
                : !item.GetSelfAndBaseTypes().Skip(1).Any(@class => !@class.Equals(target, SymbolEqualityComparer.Default)))
            .ToList();
        if (!invalidClasses.Any())
        {
            return true;
        }

        foreach (var invalidClass in invalidClasses)
        {
            foreach (var location in invalidClass.Locations)
            {
                context.ReportDiagnostic(Diagnostic.Create(ImplementTargetInterface, location, target.ToDisplayString(), invalidClass.Name));
            }
        }

        return false;
    }

    public static bool CheckNoGenericParameters(this GeneratorExecutionContext context, IEnumerable<INamedTypeSymbol> symbols)
    {
        var invalidClasses = symbols
            .Where(item => item.TypeParameters.Any())
            .ToList();
        if (!invalidClasses.Any())
        {
            return true;
        }

        foreach (var location in invalidClasses.SelectMany(item => item.Locations))
        {
            context.ReportDiagnostic(Diagnostic.Create(NoGenericParameters, location));
        }

        return false;
    }

    public static bool CheckTheSameKeyType(this GeneratorExecutionContext context, int keyCount, IEnumerable<INamedTypeSymbol> symbols)
    {
        if (keyCount <= 1)
        {
            return true;
        }

        var locations = symbols.SelectMany(item => item.Locations);
        foreach (var location in locations)
        {
            context.ReportDiagnostic(Diagnostic.Create(TheSameKeyType, location));
        }

        return false;
    }
}
