using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SimpleFactoryGenerator.SourceGenerator;

internal class ProductInfo
{
    public bool IsPrivate { get; set; }

    public string LabelValue { get; set; } = null!;

    public string ClassType { get; set; } = null!;

    public IReadOnlyList<(string Key, string Value)> Tags { get; set; } = null!;

    public static ProductInfo From(AttributeItem item)
    {
        return new ProductInfo
        {
            IsPrivate = IsPrivateClass(item.ClassType),
            LabelValue = item.LabelValue,
            ClassType = GetClassDeclaration(item.ClassType),
            Tags = item.Tags,
        };
    }

    private static bool IsPrivateClass(ISymbol symbol)
    {
        return symbol.DeclaredAccessibility is Accessibility.Private;
    }

    private static string GetClassDeclaration(ISymbol symbol)
    {
        return IsPrivateClass(symbol)
            ? $"{symbol.ContainingType.ToDisplayString()}+{symbol.Name}"
            : symbol.ToDeclaration();
    }
}
