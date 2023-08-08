using System.Collections.Generic;

namespace SimpleFactoryGenerator.SourceGenerator;

internal static class ImportTypeTemplate
{
    public static string Generate(IEnumerable<FactoryInfo<ProductInfo>> infos)
    {
        return $@"
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : Attribute {{ }}
}}
#endif

namespace SimpleFactoryGenerator.Implementation
{{
    internal static class ModuleInitializer
    {{
#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
        [System.Runtime.CompilerServices.ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
        public static void Initialize()
        {{
{infos.For(x => $@"
{x.Items.For(item => $@"
            SimpleFactory<{x.KeyTypeDeclaration}, {x.TargetInterfaceDeclaration}>.Register({item.Label}, System.Type.GetType(""{item.Product}""));
")}

")}
        }}
    }}
}}
".FormatCode();
    }
}
