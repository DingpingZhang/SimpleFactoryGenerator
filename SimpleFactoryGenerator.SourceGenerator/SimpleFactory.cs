using System.Collections.Generic;
using static SimpleFactoryGenerator.SourceGenerator.TemplateExtensions;

namespace SimpleFactoryGenerator.SourceGenerator;

internal static class SimpleFactory
{
    public static string Generate(IEnumerable<FactoryInfo<ProductInfo>> infos)
    {
        return $@"
namespace SimpleFactoryGenerator.Implementation
{{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Reflection.Obfuscation(Exclude = true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static class GeneratedSimpleFactory
    {{
{infos.For(info => $@"
        private class {info.Namespace.Replace(".", "_")}_{info.TargetInterfaceName} : ISimpleFactory<{info.KeyType}, {info.TargetInterfaceDeclaration}>
        {{
            public System.Collections.Generic.IReadOnlyCollection<{info.KeyType}> Keys {{ get; }} = new []
            {{
{info.Items.For(info => $@"
                {info.Label},
")}
            }};

            public {info.TargetInterfaceDeclaration} Create({info.KeyType} key)
            {{
                switch(key)
                {{
{info.Items.For(product => $@"
{Text(product.IsPrivate ? $@"
                    case {product.Label}:
                        return ({info.TargetInterfaceDeclaration})System.Activator.CreateInstance(System.Type.GetType(""{product.ClassDeclaration}""));
" : $@"
                    case {product.Label}:
                        return new {product.ClassDeclaration}();
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
