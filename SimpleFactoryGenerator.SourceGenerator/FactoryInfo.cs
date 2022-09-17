using System.Collections.Generic;

namespace SimpleFactoryGenerator.SourceGenerator;

internal class FactoryInfo<T>
{
    public string Namespace { get; set; } = null!;

    public string TargetInterfaceName { get; set; } = null!;

    public string TargetInterfaceDeclaration { get; set; } = null!;

    public string KeyType { get; set; } = null!;

    public IReadOnlyCollection<T> Items { get; set; } = null!;
}
