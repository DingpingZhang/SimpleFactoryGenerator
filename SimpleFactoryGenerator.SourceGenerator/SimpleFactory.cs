using System.Collections.Generic;
using static SimpleFactoryGenerator.SourceGenerator.TemplateExtensions;

namespace SimpleFactoryGenerator.SourceGenerator;

internal static class SimpleFactory
{
    public static string Generate(IEnumerable<FactoryInfo> infos)
    {
        return $@"
namespace SimpleFactoryGenerator.Implementation
{{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Reflection.Obfuscation(Exclude = true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static class Generated
    {{
{infos.For(info => $@"
        private class {info.Namespace.Replace(".", "_")}_{info.TargetInterfaceName}SimpleFactory : ISimpleFactory<{info.KeyType}, {info.TargetInterfaceDeclaration}>
        {{
            public System.Collections.Generic.IReadOnlyCollection<{info.KeyType}> Keys {{ get; }} = new []
            {{
{info.Products.For(info => $@"
                {info.Label},
")}
            }};

            public {info.TargetInterfaceDeclaration} Create({info.KeyType} key)
            {{
                switch(key)
                {{
{info.Products.For(product => $@"
{Text(product.IsPrivate ? $@"
                    case {product.Label}:
                        return ({info.TargetInterfaceDeclaration})System.Activator.CreateInstance(System.Type.GetType(""{product.ProductClassDeclaration}""));
" : $@"
                    case {product.Label}:
                        return new {product.ProductClassDeclaration}();
")}
")}
                    default:
                        throw new System.IndexOutOfRangeException($""The factory does not contain products with '{{key}}'."");
                }}
            }}
        }}

")}
    }}
}}
".FormatCode();
    }
}
