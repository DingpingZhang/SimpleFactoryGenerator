using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SimpleFactoryGenerator.SourceGenerator;

internal static class Extensions
{
    public static string ToDisplayValue(this TypedConstant constant)
    {
        /*
         * Ref to: https://stackoverflow.com/a/25859321
         * 
         * TODO:
         * The types of positional and named parameters for an attribute class are limited to the attribute parameter types, which are:
         * 
         * 1. One of the following types: bool, byte, char, double, float, int, long, sbyte, short, string, uint, ulong, ushort.
         * 2. The type object.
         * 3. The type System.Type.
         * 4. An enum type, provided it has public accessibility and the types in which it is nested (if any) also have public accessibility (Attribute specification).
         * 5. Single-dimensional arrays of the above types. (emphasis added by me)
         */

        if (constant.Type!.ToDeclaration() == "string")
        {
            return $"\"{constant.Value}\"";
        }

        if (constant.Kind is TypedConstantKind.Enum)
        {
            return $"({constant.Type!.ToDeclaration()}){constant.Value}";
        }

        return $"{constant.Value}";
    }

    public static string ToDeclaration(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static IEnumerable<ITypeSymbol> GetSelfAndBaseTypes(this ITypeSymbol symbol)
    {
        ITypeSymbol? baseType = symbol;
        while (baseType is not null)
        {
            yield return baseType;

            baseType = baseType.BaseType;
        }
    }

    public static bool EqualAttribute(this INamedTypeSymbol expected, INamedTypeSymbol? actual)
    {
        return actual is { IsGenericType: true, IsUnboundGenericType: false } &&
               expected.Equals(actual.ConstructUnboundGenericType(), SymbolEqualityComparer.Default) ||
               expected.Equals(actual, SymbolEqualityComparer.Default);
    }
}
