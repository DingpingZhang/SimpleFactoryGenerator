using System.Collections.Generic;
using static SimpleFactoryGenerator.SourceGenerator.TemplateExtensions;

namespace SimpleFactoryGenerator.SourceGenerator;

internal static class Factory
{
    public static string Generate(IEnumerable<FactoryInfo<CreatorInfo>> infos)
    {
        return $@"
namespace SimpleFactoryGenerator.Implementation
{{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Reflection.Obfuscation(Exclude = true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static class GeneratedFactory
    {{
{infos.For(info =>
        {
            string creatorType = $"ICreator<{info.KeyType}, {info.TargetInterfaceDeclaration}>";
            return $@"
        private class {info.Namespace.Replace(".", "_")}_{info.TargetInterfaceName} : IFactory<{info.KeyType}, {info.TargetInterfaceDeclaration}>
        {{
            public System.Collections.Generic.IReadOnlyCollection<{creatorType}> Creators {{ get; }} = new []
            {{
{info.Items.For(creator => $@"
{Text(creator.IsPrivate ? $@"
                ({creatorType})System.Activator.CreateInstance(System.Type.GetType(""{creator.ClassDeclaration}"")),
" : $@"
                new {creator.ClassDeclaration}(),
")}
")}
            }};
        }}

";
        })}
    }}
}}
".FormatCode();
    }
}
