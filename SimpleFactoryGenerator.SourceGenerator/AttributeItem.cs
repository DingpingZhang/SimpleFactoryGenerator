using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SimpleFactoryGenerator.SourceGenerator;

internal class AttributeItem
{
    public static INamedTypeSymbol ProductSymbol { get; private set; } = null!;

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

    public static IEnumerable<AttributeItem> From(INamedTypeSymbol classSymbol)
    {
        const int labelIndex = 0;

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

            var ctorArgs = attribute.ConstructorArguments;
            string labelValue = ctorArgs[labelIndex].ToDisplayValue();

            yield return new AttributeItem
            {
                LabelType = GetLabelType(productSymbol),
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
        foreach (var (x, y) in parameters.Zip(arguments, (x, y) => (x, y)).Skip(1))
        {
            yield return (x.Name, y.ToDisplayValue());
        }
    }

    private static IEnumerable<(string key, string value)> GetTagsFromProperty(AttributeData attribute)
    {
        return attribute.NamedArguments.Select(x => (x.Key, x.Value.ToDisplayValue()));
    }
}
