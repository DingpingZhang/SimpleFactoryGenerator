using System.Collections.Generic;

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
            public {info.TargetInterfaceDeclaration} Create({info.KeyType} key)
            {{
                return key switch
                {{
{info.Products.For(product => $@"
                    {product.Label} => ({info.TargetInterfaceDeclaration})new {product.ProductClassDeclaration}(),
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
