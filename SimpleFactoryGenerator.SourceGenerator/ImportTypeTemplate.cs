using System.Collections.Generic;
using static SimpleFactoryGenerator.SourceGenerator.TemplateExtensions;

namespace SimpleFactoryGenerator.SourceGenerator;

internal static class ImportTypeTemplate
{
    public static string Generate(IEnumerable<FactoryInfo> infos)
    {
        return $@"
#nullable enable

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
{x.Items.For(item => Text(item.IsPrivate ? $@"
            SimpleFactory<{x.LabelType}, {x.InterfaceType}>.Register({item.LabelValue}, System.Type.GetType(""{item.ClassType}""), {GetTagsCode(item.Tags)});
" : $@"
            SimpleFactory<{x.LabelType}, {x.InterfaceType}>.Register({item.LabelValue}, typeof({item.ClassType}), {GetTagsCode(item.Tags)});
"))}

")}
        }}

        private sealed class Tags : SimpleFactoryGenerator.ITags
        {{
            public static readonly SimpleFactoryGenerator.ITags Empty = new Tags(new System.Collections.Generic.Dictionary<string, object?>());

            private readonly System.Collections.Generic.IReadOnlyDictionary<string, object?> _storage;

            public int Count => _storage.Count;

            public Tags(System.Collections.Generic.IReadOnlyDictionary<string, object?> storage) => _storage = storage;

            public bool Contains(string name) => _storage.ContainsKey(name);

            public T GetValue<T>(string name) => (T)_storage[name]!;
        }}
    }}
}}
".FormatCode();
    }

    private static string GetTagsCode(IReadOnlyList<(string Key, string Value)> tags)
    {
        return tags.Count is 0
            ? "Tags.Empty"
            : $@"new Tags(new System.Collections.Generic.Dictionary<string, object?>{{ {tags.Join(", ", x => $@"{{ ""{x.Key}"", {x.Value} }}")} }})";
    }
}
