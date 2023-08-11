using SimpleFactoryGenerator.Tests.Other;

namespace SimpleFactoryGenerator.Tests;

[ProductCrossAssembly(nameof(Product1InThisAssembly))]
internal class Product1InThisAssembly : IProductCrossAssembly
{
    public string Name { get; }

    public Product1InThisAssembly(string name) => Name = name;
}
