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

        if (!AttributeItem.Initialize(compilation))
        {
            return;
        }

        var attributeItems = receiver.CandidateClasses
            .GroupBy(@class => @class.SyntaxTree)
            .SelectMany(group =>
            {
                SemanticModel model = compilation.GetSemanticModel(group.Key);
                return group
                    .Select(x => model.GetDeclaredSymbol(x))
                    .OfType<INamedTypeSymbol>()
                    .SelectMany(AttributeItem.From);
            })
            .ToArray();

        var classSymbols = attributeItems
            .Select(x => x.ClassType)
            .Distinct(SymbolEqualityComparer.Default)
            .OfType<INamedTypeSymbol>()
            .ToArray();
        if (!context.CheckNoGenericParameters(classSymbols))
        {
            return;
        }

        var factoryInfos = GetFactories(context, attributeItems).ToArray();
        if (factoryInfos.Any())
        {
            string simpleFactorySource = ImportTypeTemplate.Generate(factoryInfos);
            context.AddSource("ImportType.g.cs", SourceText.From(simpleFactorySource, Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static IEnumerable<FactoryInfo> GetFactories(GeneratorExecutionContext context, IEnumerable<AttributeItem> attributeItems)
    {
        var groups = attributeItems.GroupBy(x => x.InterfaceType);

        foreach (var group in groups)
        {
            var target = group.Key;
            var items = group.ToArray();
            var classTypes = items.Select(x => x.ClassType);
            if (!context.CheckImplementTargetInterface(target, classTypes))
            {
                yield break;
            }

            var keyTypes = items.Select(x => x.LabelType).Distinct(SymbolEqualityComparer.Default).ToArray();
            if (!context.CheckTheSameKeyType(keyTypes.Length, classTypes))
            {
                yield break;
            }

            string labelType = keyTypes[0]!.ToDeclaration();
            string interfaceType = target.ToDeclaration();

            yield return new FactoryInfo
            {
                LabelType = labelType,
                InterfaceType = interfaceType,
                Items = items.Select(ProductInfo.From).ToArray(),
            };
        }
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
