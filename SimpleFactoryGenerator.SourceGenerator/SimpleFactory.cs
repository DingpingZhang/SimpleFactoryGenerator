using System.Collections.Generic;
using static SimpleFactoryGenerator.SourceGenerator.TemplateExtensions;

namespace SimpleFactoryGenerator.SourceGenerator
{
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
        private class {info.Namespace.Replace(".", "_")}_{info.TargetInterfaceName}Factory : ISimpleFactory<{info.TargetInterfaceDeclaration}, {info.KeyType}>
        {{
            public System.Collections.Generic.IReadOnlyCollection<{info.KeyType}> Keys {{ get; }} = new []
            {{
{info.Products.For(info => $@"
                {info.Label},
")}
            }};

            public {info.TargetInterfaceDeclaration} Create({info.KeyType} key)
            {{
                return key switch
                {{
{info.Products.For(product => $@"
{Text(product.IsPrivate ? $@"
                    {product.Label} => ({info.TargetInterfaceDeclaration})System.Activator.CreateInstance(System.Type.GetType(""{product.ProductClassDeclaration}"")),
" : $@"
                    {product.Label} => ({info.TargetInterfaceDeclaration})new {product.ProductClassDeclaration}(),
")}
")}
                    _ => throw new System.IndexOutOfRangeException($""The factory does not contain products with '{{key}}'.""),
                }};
            }}
        }}

")}
    }}
}}
".FormatCode();
        }
    }
}
