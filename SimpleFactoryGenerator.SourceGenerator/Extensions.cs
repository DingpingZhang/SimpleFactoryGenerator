using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SimpleFactoryGenerator.SourceGenerator
{
    internal static class Extensions
    {
        public static string ToDisplayValue(this TypedConstant constant)
        {
            return constant.Type!.ToDeclaration() is "string"
                ? $"\"{constant.Value}\""
                : $"{constant.Value}";
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

                baseType = symbol.BaseType;
            }
        }

        public static IEnumerable<(TypedConstant label, INamedTypeSymbol type)> GetAttributes(this INamedTypeSymbol symbol, INamedTypeSymbol attributeSymbol)
        {
            const int argumentIndex = 0;

            return from attribute in symbol.GetAttributes()
                   let type = attribute.AttributeClass
                   let productAttribute = type.GetSelfAndBaseTypes()
                       .OfType<INamedTypeSymbol>()
                       .FirstOrDefault(item => attributeSymbol.EqualAttribute(item))
                   where productAttribute != null
                   let label = attribute.ConstructorArguments[argumentIndex]
                   select (label, productAttribute);
        }

        public static bool EqualAttribute(this INamedTypeSymbol expected, INamedTypeSymbol? actual)
        {
            return actual is { IsGenericType: true, IsUnboundGenericType: false } &&
                   expected.Equals(actual.ConstructUnboundGenericType(), SymbolEqualityComparer.Default) ||
                   expected.Equals(actual, SymbolEqualityComparer.Default);
        }

        public static IEnumerable<INamedTypeSymbol> FilterNoParameterlessCtorClasses(this IEnumerable<INamedTypeSymbol> classes)
        {
            return classes.Where(item => !item.Constructors.Any(ctor => !ctor.Parameters.Any()));
        }
    }
}
