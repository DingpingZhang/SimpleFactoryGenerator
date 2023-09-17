using System.Collections.Generic;

namespace SimpleFactoryGenerator.SourceGenerator;

internal class FactoryInfo
{
    public string InterfaceType { get; set; } = null!;

    public string LabelType { get; set; } = null!;

    public IReadOnlyCollection<ProductInfo> Items { get; set; } = null!;
}
