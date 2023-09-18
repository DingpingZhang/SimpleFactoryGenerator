namespace SimpleFactoryGenerator.Tests.Other;

[ProductCrossAssembly(nameof(Product1InOtherAssembly))]
internal class Product1InOtherAssembly : IProductCrossAssembly
{
    public string Name { get; }

    public Product1InOtherAssembly(string name) => Name = name;
}

public class TestAttribute(int key) : ProductAttribute<int, IProductCrossAssembly>(key)
{
}
