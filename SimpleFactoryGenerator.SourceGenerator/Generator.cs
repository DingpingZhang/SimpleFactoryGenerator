using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static SimpleFactoryGenerator.SourceGenerator.DiagnosticDescriptors;

namespace SimpleFactoryGenerator.SourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            Compilation compilation = context.Compilation;

            INamedTypeSymbol? genericAttributeSymbol = compilation
                .GetTypeByMetadataName("SimpleFactoryGenerator.ProductOfSimpleFactoryAttribute`2")?
                .ConstructUnboundGenericType();
            INamedTypeSymbol? attributeSymbol = compilation
                .GetTypeByMetadataName("SimpleFactoryGenerator.ProductOfSimpleFactoryAttribute");
            if (genericAttributeSymbol is null || attributeSymbol is null)
            {
                return;
            }

            var productClasses = receiver.CandidateClasses
                .GroupBy(@class => @class.SyntaxTree)
                .SelectMany(group =>
                {
                    SemanticModel model = compilation.GetSemanticModel(group.Key);
                    return group
                        .Select(@class => model.GetDeclaredSymbol(@class))
                        .Where(@class => @class is not null)
                        // WORKAROUND: The @interface must not be null here.
                        .Select(@class => @class!)
                        .Select(@class => (
                            @class,
                            genericAttributes: @class.GetAttributes(genericAttributeSymbol).ToList(),
                            attributes: @class.GetAttributes(attributeSymbol).ToList()))
                        .Where(item => item.genericAttributes.Any() || item.attributes.Any());
                })
                .ToList();

            if (!productClasses.Any())
            {
                return;
            }

            var invalidClasses = productClasses.Select(item => item.@class).FilterNoParameterlessCtorClasses().ToList();
            if (invalidClasses.Any())
            {
                foreach (var location in invalidClasses.SelectMany(item => item.Locations))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ParameterlessConstructor, location));
                }

                return;
            }

            invalidClasses = productClasses.Select(item => item.@class).Where(item => item.TypeParameters.Any()).ToList();
            if (invalidClasses.Any())
            {
                foreach (var location in invalidClasses.SelectMany(item => item.Locations))
                {
                    context.ReportDiagnostic(Diagnostic.Create(NoGenericParameters, location));
                }

                return;
            }

            var groups = productClasses
                .SelectMany(info => info.genericAttributes
                    .Select(attribute => (
                        info.@class,
                        attribute,
                        target: GetTargetSymbolFromGeneric(attribute.type)))
                    .Concat(info.attributes
                    .Select(attribute => (
                        info.@class,
                        attribute,
                        target: GetTargetSymbol(attribute.ctorArgs)))))
                .GroupBy(item => item.target)
                .ToList();

            var infos = new List<FactoryInfo>();
            foreach (var group in groups)
            {
                var groupList = group.ToList();
                var target = group.Key;

                var keyTypes = groupList
                    .Select(item => GetKeyDeclaration(item.attribute))
                    .Distinct()
                    .ToList();

                if (keyTypes.Count > 1)
                {
                    var locations = groupList.SelectMany(item => item.@class.Locations);
                    foreach (var location in locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(TheSameKeyType, location));
                    }

                    return;
                }

                invalidClasses = groupList
                    .Select(item => item.@class)
                    .Where(item => !item.AllInterfaces.Any(@interface => @interface.Equals(target, SymbolEqualityComparer.Default)))
                    .ToList();

                if (invalidClasses.Any())
                {
                    foreach (var invalidClass in invalidClasses)
                    {
                        foreach (var location in invalidClass.Locations)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(ImplementTargetInterface, location, target.Name, invalidClass.Name));
                        }
                    }

                    return;
                }

                var labelToClasses = groupList.Select(item =>
                {
                    bool isPrivate = item.@class.DeclaredAccessibility is Accessibility.Private;
                    return new ProductInfo
                    {
                        Label = GetLabel(item.attribute),
                        IsPrivate = isPrivate,
                        ProductClassDeclaration = isPrivate
                            ? $"{item.@class.ContainingType.ToDisplayString()}+{item.@class.Name}"
                            : item.@class.ToDeclaration(),
                    };
                });

                infos.Add(new FactoryInfo
                {
                    Namespace = target.ContainingNamespace.ToDisplayString(),
                    TargetInterfaceName = target.Name,
                    TargetInterfaceDeclaration = target.ToDeclaration(),
                    KeyType = keyTypes.Single(),
                    Products = labelToClasses.ToList(),
                });
            }

            string source = SimpleFactory.Generate(infos);
            context.AddSource($"SimpleFactory.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private static ITypeSymbol GetTargetSymbolFromGeneric(INamedTypeSymbol symbol)
        {
            const int targetTypeIndex = 0;

            return symbol.TypeArguments[targetTypeIndex];
        }

        private static ITypeSymbol GetTargetSymbol(IReadOnlyList<TypedConstant> constants)
        {
            const int targetTypeIndex = 0;

            return (ITypeSymbol)constants[targetTypeIndex].Value!;
        }

        private static string GetKeyDeclaration((IReadOnlyList<TypedConstant> ctorArgs, INamedTypeSymbol type) info)
        {
            const int keyTypeIndex = 1;

            var symbol = info.type.IsGenericType
                ? info.type.TypeArguments[keyTypeIndex]
                : (ITypeSymbol)info.ctorArgs[keyTypeIndex].Value!;

            return symbol.ToDeclaration();
        }

        private static string GetLabel((IReadOnlyList<TypedConstant> ctorArgs, INamedTypeSymbol type) info)
        {
            const int genericKeyIndex = 0;
            const int keyIndex = 2;

            return info.ctorArgs[info.type.IsGenericType ? genericKeyIndex : keyIndex].ToDisplayValue();
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } interfaceDeclarationSyntax)
                {
                    CandidateClasses.Add(interfaceDeclarationSyntax);
                }
            }
        }
    }
}
