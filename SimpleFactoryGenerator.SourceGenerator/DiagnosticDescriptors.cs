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

    public static readonly DiagnosticDescriptor ParameterlessConstructor = new(
        "SFG002",
        "The class must have a parameterless constructor",
        "The product or creator class must have a parameterless constructor, consider adding the parameterless constructor",
        SimpleFactoryGenerator,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor TheSameKeyType = new(
        "SFG003",
        "Classes with the same target interface must have the same key type",
        "If the target interfaces are the same, their key types must also be the same, consider using the same type of key",
        SimpleFactoryGenerator,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor ImplementTargetInterface = new(
        "SFG004",
        "The class must implement the target interface declared by the attribute",
        "The product class must implement the target interface declared on the attribute, consider implementing the interface '{0}' for class '{1}'",
        SimpleFactoryGenerator,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor InTheSameAssembly = new(
        "SFG005",
        "The class must be in the same assembly as the interface",
        "The class '{0}' is not in the same assembly as the interface '{1}' it implements, consider placing it in the same assembly.",
        SimpleFactoryGenerator,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor ImplementCreatorInterface = new(
        "SFG006",
        "The class must implement the target interface declared by the attribute",
        "The creator class must implement the ICreator<,> interface declared on the attribute, consider implementing the interface '{0}' for class '{1}'",
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
        if (invalidClasses.Any())
        {
            foreach (var invalidClass in invalidClasses)
            {
                foreach (var location in invalidClass.Locations)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ImplementTargetInterface, location, target.ToDisplayString(), invalidClass.Name));
                }
            }

            return false;
        }

        return true;
    }

    public static bool CheckTheSameAssembly(this GeneratorExecutionContext context, ITypeSymbol target, IEnumerable<INamedTypeSymbol> symbols)
    {
        var invalidClasses = symbols
            .Where(item => !item.ContainingAssembly.Equals(target.ContainingAssembly, SymbolEqualityComparer.Default))
            .ToList();
        if (invalidClasses.Any())
        {
            foreach (var invalidClass in invalidClasses)
            {
                foreach (var location in invalidClass.Locations)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InTheSameAssembly, location, invalidClass.Name, target.Name));
                }
            }

            return false;
        }

        return true;
    }

    public static bool CheckNoGenericParameters(this GeneratorExecutionContext context, IEnumerable<INamedTypeSymbol> symbols)
    {
        var invalidClasses = symbols
            .Where(item => item.TypeParameters.Any())
            .ToList();
        if (invalidClasses.Any())
        {
            foreach (var location in invalidClasses.SelectMany(item => item.Locations))
            {
                context.ReportDiagnostic(Diagnostic.Create(NoGenericParameters, location));
            }

            return false;
        }

        return true;
    }

    public static bool CheckParameterlessConstructor(this GeneratorExecutionContext context, IEnumerable<INamedTypeSymbol> symbols)
    {
        var invalidClasses = symbols
            .FilterNoParameterlessCtorClasses()
            .ToList();
        if (invalidClasses.Any())
        {
            foreach (var location in invalidClasses.SelectMany(item => item.Locations))
            {
                context.ReportDiagnostic(Diagnostic.Create(ParameterlessConstructor, location));
            }

            return false;
        }

        return true;
    }

    public static bool CheckTheSameKeyType(this GeneratorExecutionContext context, int keyCount, IEnumerable<INamedTypeSymbol> symbols)
    {
        if (keyCount > 1)
        {
            var locations = symbols.SelectMany(item => item.Locations);
            foreach (var location in locations)
            {
                context.ReportDiagnostic(Diagnostic.Create(TheSameKeyType, location));
            }

            return false;
        }

        return true;
    }

    public static bool CheckImplementCreatorInterface(this GeneratorExecutionContext context, ITypeSymbol target, IEnumerable<INamedTypeSymbol> symbols)
    {
        var invalidClasses = symbols
            .Where(item => target.TypeKind is TypeKind.Interface
                // Inherited from interface
                ? !item.AllInterfaces.Any(@interface => @interface.Equals(target, SymbolEqualityComparer.Default))
                // Inherited from class
                : !item.GetSelfAndBaseTypes().Skip(1).Any(@class => !@class.Equals(target, SymbolEqualityComparer.Default)))
            .ToList();
        if (invalidClasses.Any())
        {
            foreach (var invalidClass in invalidClasses)
            {
                foreach (var location in invalidClass.Locations)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ImplementCreatorInterface, location, target.ToDisplayString(), invalidClass.Name));
                }
            }

            return false;
        }

        return true;
    }
}
