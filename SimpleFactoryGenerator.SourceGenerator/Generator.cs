using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

        INamedTypeSymbol? productSymbol = compilation
            .GetTypeByMetadataName("SimpleFactoryGenerator.ProductAttribute`2")?
            .ConstructUnboundGenericType();

        if (productSymbol is null)
        {
            return;
        }

        var markedClasses = receiver.CandidateClasses
            .GroupBy(@class => @class.SyntaxTree)
            .SelectMany(group =>
            {
                SemanticModel model = compilation.GetSemanticModel(group.Key);
                return group
                    .Select(@class => model.GetDeclaredSymbol(@class))
                    .OfType<INamedTypeSymbol>()
                    .Select(@class => (
                        @class,
                        attributes: @class.GetAttributes(productSymbol).ToList()))
                    .Where(item => item.attributes.Any());
            })
            .ToList();

        if (!markedClasses.Any())
        {
            return;
        }

        if (!context.CheckNoGenericParameters(markedClasses.Select(item => item.@class)))
        {
            return;
        }

        var groups = markedClasses
            .SelectMany(info => info.attributes
                .Select(attribute => (
                    info.@class,
                    attribute,
                    target: GetTargetType(attribute.type))))
            .GroupBy(item => item.target)
            .ToList();

        var productInfos = new List<FactoryInfo<ProductInfo>>();
        foreach (var group in groups)
        {
            var groupList = group.ToList();
            var target = group.Key;

            var keyTypes = groupList
                .Select(item => GetKeyType(item.attribute.type))
                .Distinct(SymbolEqualityComparer.Default)
                .ToArray();
            if (!context.CheckTheSameKeyType(keyTypes.Length, groupList.Select(item => item.@class)))
            {
                return;
            }

            var productClasses = groupList
                .Where(item => productSymbol.EqualAttribute(item.attribute.type))
                .ToArray();
            if (!context.CheckImplementTargetInterface(target, productClasses.Select(item => item.@class)))
            {
                return;
            }

            ISymbol keyType = keyTypes.Single()!;

            string keyTypeDeclaration = keyType.ToDeclaration();
            if (productClasses.Any())
            {
                var products = productClasses
                    .Select(item => new ProductInfo
                    {
                        Label = GetLabel(item.attribute.ctorArgs),
                        Product = GetClassDeclaration(item.@class),
                    })
                    .ToArray();
                productInfos.Add(CreateFactoryInfo(target, keyTypeDeclaration, products));
            }
        }

        if (productInfos.Any())
        {
            string simpleFactorySource = ImportTypeTemplate.Generate(productInfos);
            context.AddSource("ImportType.g.cs", SourceText.From(simpleFactorySource, Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static FactoryInfo<T> CreateFactoryInfo<T>(ITypeSymbol target, string keyType, IReadOnlyCollection<T> items)
    {
        return new FactoryInfo<T>
        {
            TargetInterfaceDeclaration = target.ToDeclaration(),
            KeyTypeDeclaration = keyType,
            Items = items,
        };
    }

    private static string GetClassDeclaration(ITypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility is Accessibility.Private
            ? $"{symbol.ContainingType.ToDisplayString()}+{symbol.Name}"
            : symbol.ToDisplayString();
    }

    private static ITypeSymbol GetKeyType(INamedTypeSymbol attribute)
    {
        const int keyTypeIndex = 0;

        // e.g. Creator<*TKey*, TProduct> or Product<*TKey*, TProduct>.
        return attribute.TypeArguments[keyTypeIndex];
    }

    private static ITypeSymbol GetTargetType(INamedTypeSymbol symbol)
    {
        const int targetTypeIndex = 1;

        // e.g. Creator<TKey, *TProduct*> or Product<TKey, *TProduct*>.
        return symbol.TypeArguments[targetTypeIndex];
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
