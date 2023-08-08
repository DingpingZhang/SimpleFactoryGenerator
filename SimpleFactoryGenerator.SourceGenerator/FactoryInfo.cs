using System.Collections.Generic;

namespace SimpleFactoryGenerator.SourceGenerator;

internal class FactoryInfo<T>
{
    public string TargetInterfaceDeclaration { get; set; } = null!;

    public string KeyTypeDeclaration { get; set; } = null!;

    public IReadOnlyCollection<T> Items { get; set; } = null!;
}
