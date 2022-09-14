using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using static SimpleFactoryGenerator.SourceGenerator.DiagnosticDescriptors;

namespace SimpleFactoryGenerator.SourceGenerator;

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

        INamedTypeSymbol? simpleProductSymbol = compilation
            .GetTypeByMetadataName("SimpleFactoryGenerator.ProductAttribute`2")?
            .ConstructUnboundGenericType();
        INamedTypeSymbol? productSymbol = compilation
            .GetTypeByMetadataName("SimpleFactoryGenerator.ProductAttribute`3")?
            .ConstructUnboundGenericType();

        if (simpleProductSymbol is null || productSymbol is null)
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
                    .OfType<INamedTypeSymbol>()
                    .Select(@class => (
                        @class,
                        attributes: @class.GetAttributes(simpleProductSymbol, productSymbol).ToList()))
                    .Where(item => item.attributes.Any());
            })
            .ToList();

        if (!productClasses.Any())
        {
            return;
        }

        var invalidClasses = productClasses
            // Only simple-factory-product require the parameterless constructor, while factory-method-product are created by the Creator,
            // so this check is not required.
            .Where(item => item.attributes.Any(attribute => attribute.type.EqualAttribute(simpleProductSymbol)))
            .Select(item => item.@class)
            .FilterNoParameterlessCtorClasses()
            .ToList();
        if (invalidClasses.Any())
        {
            foreach (var location in invalidClasses.SelectMany(item => item.Locations))
            {
                context.ReportDiagnostic(Diagnostic.Create(ParameterlessConstructor, location));
            }

            return;
        }

        invalidClasses = productClasses
            .Where(item => item.attributes.Any(attribute => attribute.type.EqualAttribute(simpleProductSymbol)))
            .Select(item => item.@class)
            .Where(item => item.TypeParameters.Any())
            .ToList();
        if (invalidClasses.Any())
        {
            foreach (var location in invalidClasses.SelectMany(item => item.Locations))
            {
                context.ReportDiagnostic(Diagnostic.Create(NoGenericParameters, location));
            }

            return;
        }

        var groups = productClasses
            .SelectMany(info => info.attributes
                .Select(attribute => (
                    info.@class,
                    attribute,
                    target: GetTargetType(attribute.type))))
            .GroupBy(item => item.target)
            .ToList();

        var productInfos = new List<FactoryInfo<ProductInfo>>();
        var creatorInfos = new List<FactoryInfo<CreatorInfo>>();
        foreach (var group in groups)
        {
            var groupList = group.ToList();
            var target = group.Key;

            var keyTypes = groupList
                .Select(item => GetKeyType(item.attribute.type).ToDeclaration())
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
                        context.ReportDiagnostic(Diagnostic.Create(ImplementTargetInterface, location, target.Name, invalidClass.Name));
                    }
                }

                return;
            }

            invalidClasses = groupList
                .Select(item => item.@class)
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

                return;
            }

            var products = groupList
                .Where(item => simpleProductSymbol.EqualAttribute(item.attribute.type))
                .Select(item =>
                {
                    bool isPrivate = item.@class.DeclaredAccessibility is Accessibility.Private;
                    return new ProductInfo
                    {
                        Label = GetLabel(item.attribute.ctorArgs),
                        IsPrivate = isPrivate,
                        ClassDeclaration = isPrivate
                            ? $"{item.@class.ContainingType.ToDisplayString()}+{item.@class.Name}"
                            : item.@class.ToDeclaration(),
                    };
                })
                .ToList();
            if (products.Any())
            {
                productInfos.Add(new FactoryInfo<ProductInfo>
                {
                    Namespace = target.ContainingNamespace.ToDisplayString(),
                    TargetInterfaceName = target.Name,
                    TargetInterfaceDeclaration = target.ToDeclaration(),
                    KeyType = keyTypes.Single(),
                    Items = products.ToList(),
                });
            }

            var creators = groupList
                .Where(item => productSymbol.EqualAttribute(item.attribute.type))
                .Select(item =>
                {
                    ITypeSymbol creatorType = GetCreatorType(item.attribute.type);
                    bool isPrivate = creatorType.DeclaredAccessibility is Accessibility.Private;
                    return new CreatorInfo
                    {
                        IsPrivate = isPrivate,
                        ClassDeclaration = isPrivate
                            ? $"{creatorType.ContainingType.ToDisplayString()}+{creatorType.Name}"
                            : creatorType.ToDeclaration(),
                    };
                })
                .ToList();
            if (creators.Any())
            {
                creatorInfos.Add(new FactoryInfo<CreatorInfo>
                {
                    Namespace = target.ContainingNamespace.ToDisplayString(),
                    TargetInterfaceName = target.Name,
                    TargetInterfaceDeclaration = target.ToDeclaration(),
                    KeyType = keyTypes.Single(),
                    Items = creators.ToList(),
                });
            }
        }

        if (productInfos.Any())
        {
            string simpleFactorySource = SimpleFactory.Generate(productInfos);
            context.AddSource($"SimpleFactory.g.cs", SourceText.From(simpleFactorySource, Encoding.UTF8));
        }

        if (creatorInfos.Any())
        {
            string factoryMethodSource = Factory.Generate(creatorInfos);
            context.AddSource($"Factory.g.cs", SourceText.From(factoryMethodSource, Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static ITypeSymbol GetKeyType(INamedTypeSymbol attribute)
    {
        const int keyTypeIndex = 0;

        // e.g. Product<*TKey*, TProduct, TCreator> or Product<*TKey*, TProduct>.
        return attribute.TypeArguments[keyTypeIndex];
    }

    private static ITypeSymbol GetTargetType(INamedTypeSymbol symbol)
    {
        const int targetTypeIndex = 1;

        // e.g. Product<TKey, *TProduct*, TCreator> or Product<TKey, *TProduct*>.
        return symbol.TypeArguments[targetTypeIndex];
    }

    private static ITypeSymbol GetCreatorType(INamedTypeSymbol symbol)
    {
        const int creatorTypeIndex = 2;

        // e.g. Product<TKey, TProduct, *TCreator*>.
        return symbol.TypeArguments[creatorTypeIndex];
    }

    private static string GetLabel(IReadOnlyList<TypedConstant> ctorArgs)
    {
        const int labelIndex = 0;

        return ctorArgs[labelIndex].ToDisplayValue();
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } interfaceDeclarationSyntax)
            {
                CandidateClasses.Add(interfaceDeclarationSyntax);
            }
        }
    }
}
