using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleFactoryGenerator.SourceGenerator;

internal class AttributeItem
{
    private static INamedTypeSymbol ProductSymbol { get; set; } = null!;

    public ITypeSymbol LabelType { get; set; } = null!;

    public ITypeSymbol InterfaceType { get; set; } = null!;

    public INamedTypeSymbol ClassType { get; set; } = null!;

    public string LabelValue { get; set; } = string.Empty;

    public IReadOnlyList<(string Key, string Value)> Tags { get; set; } = null!;

    public static bool Initialize(Compilation compilation)
    {
        INamedTypeSymbol? productSymbol = compilation
            .GetTypeByMetadataName("SimpleFactoryGenerator.ProductAttribute`2")?
            .ConstructUnboundGenericType();
        ProductSymbol = productSymbol!;
        return productSymbol != null;
    }

    public static IEnumerable<AttributeItem> From(GeneratorExecutionContext context, INamedTypeSymbol classSymbol)
    {
        foreach (AttributeData attribute in classSymbol.GetAttributes())
        {
            INamedTypeSymbol? symbol = attribute.AttributeClass;

            INamedTypeSymbol? productSymbol = symbol?.GetSelfAndBaseTypes()
                .OfType<INamedTypeSymbol>()
                .FirstOrDefault(ProductSymbol.EqualAttribute);
            if (productSymbol is null)
            {
                continue;
            }

            var labelType = GetLabelType(productSymbol);
            var ctorArgs = attribute.ConstructorArguments;
            if (ctorArgs.Length < 1)
            {
                context.ReportCorrectAttributeCtor(attribute, labelType);
                continue;
            }

            var label = ctorArgs[0];
            if (!SymbolEqualityComparer.Default.Equals(label.Type, labelType))
            {
                context.ReportCorrectAttributeCtor(attribute, labelType);
                continue;
            }

            string labelValue = label.ToDisplayValue();

            yield return new AttributeItem
            {
                LabelType = labelType,
                InterfaceType = GetInterfaceType(productSymbol),
                ClassType = classSymbol,
                LabelValue = labelValue,
                Tags = GetTagsFromCtor(attribute).Concat(GetTagsFromProperty(attribute)).ToArray(),
            };
        }
    }

    private static ITypeSymbol GetLabelType(INamedTypeSymbol symbol)
    {
        const int keyTypeIndex = 0;

        // e.g. ProductAttribute<*TKey*, TProduct>.
        return symbol.TypeArguments[keyTypeIndex];
    }

    private static ITypeSymbol GetInterfaceType(INamedTypeSymbol symbol)
    {
        const int targetTypeIndex = 1;

        // e.g. ProductAttribute<TKey, *TProduct*>.
        return symbol.TypeArguments[targetTypeIndex];
    }

    private static IEnumerable<(string key, string value)> GetTagsFromCtor(AttributeData attribute)
    {
        var parameters = attribute.AttributeConstructor!.Parameters;
        var arguments = attribute.ConstructorArguments;
        // skip the 'key' argument.
        foreach (var (x, y) in parameters.Zip(arguments, (x, y) => (x, y)).Skip(1))
        {
            yield return (x.Name, y.ToDisplayValue());
        }
    }

    private static IEnumerable<(string key, string value)> GetTagsFromProperty(AttributeData attribute)
    {
        var props = attribute.AttributeClass!.GetMembers().OfType<IPropertySymbol>().ToArray();
        var propNames = props.Select(x => x.Name).ToArray();
        var defaultValues = props
            .Select(x => (name: x.Name, value: GetDefaultValue(x)))
            .Where(x => !string.IsNullOrEmpty(x.value))
            .ToDictionary(x => x.name, x => x.value!);
        var values = attribute.NamedArguments.ToDictionary(x => x.Key, x => x.Value.ToDisplayValue());

        foreach (string propName in propNames)
        {
            if (values.TryGetValue(propName, out string value))
            {
                yield return (propName, value);
            }
            else if (defaultValues.TryGetValue(propName, out value))
            {
                yield return (propName, value);
            }
            else
            {
                yield return (propName, "default!");
            }
        }

        static string? GetDefaultValue(IPropertySymbol property)
        {
            if (property.DeclaringSyntaxReferences.Length != 1)
            {
                return null;
            }

            if (property.DeclaringSyntaxReferences[0].GetSyntax() is not PropertyDeclarationSyntax syntax)
            {
                return null;
            }

            return syntax.Initializer is { } initializer
                ? GetDisplayValue(property.Type, initializer.Value)
                : null;
        }

        static string GetDisplayValue(ITypeSymbol type, ExpressionSyntax syntax)
        {
            return type switch
            {
                { TypeKind: TypeKind.Enum } => $"{type.ContainingSymbol.ToDeclaration()}.{syntax}",
                _ => syntax.ToString(),
            };
        }
    }
}
