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

    public static readonly DiagnosticDescriptor CorrectAttributeCtor = new(
        "SFG004",
        "The first argument to the constructor of product attribute must be the 'Key'",
        "The product attribute must have at least one parameter that is the Key used to create the product, consider implementing an `{0}({1} key)` constructor",
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

    public static bool CheckTheSameKeyType(this GeneratorExecutionContext context, IEnumerable<AttributeItem> items)
    {
        var issues = items
            .GroupBy(x => x.ClassType)
            .Select(x => x.ToArray())
            .Where(x => x.Length > 1)
            .Where(x => x.Select(y => y.LabelType).Distinct(SymbolEqualityComparer.Default).Count() > 1)
            .ToArray();
        if (issues.Length is 0)
        {
            return true;
        }

        var locations = issues.SelectMany(x => x.SelectMany(y => y.ClassType.Locations)).Distinct();
        foreach (var location in locations)
        {
            context.ReportDiagnostic(Diagnostic.Create(TheSameKeyType, location));
        }

        return false;
    }

    public static void ReportCorrectAttributeCtor(this GeneratorExecutionContext context, AttributeData attribute, ITypeSymbol labelType)
    {
        if (attribute.AttributeClass is null)
        {
            return;
        }

        foreach (var location in attribute.AttributeClass.Locations)
        {
            context.ReportDiagnostic(Diagnostic.Create(CorrectAttributeCtor, location, attribute.AttributeClass.Name, labelType.ToDisplayString()));
        }
    }
}
