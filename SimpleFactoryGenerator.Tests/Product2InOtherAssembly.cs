using SimpleFactoryGenerator.Tests.Other;

namespace SimpleFactoryGenerator.Tests;

[ProductCrossAssembly(nameof(Product2InOtherAssembly))]
internal class Product2InOtherAssembly : IProductCrossAssembly
{
    public string Name { get; }

    public Product2InOtherAssembly(string name) => Name = name;
}
