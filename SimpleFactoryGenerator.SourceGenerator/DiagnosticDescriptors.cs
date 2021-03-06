using Microsoft.CodeAnalysis;

namespace SimpleFactoryGenerator.SourceGenerator
{
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
            "The product class must have a parameterless constructor, consider adding the parameterless constructor",
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
    }
}
