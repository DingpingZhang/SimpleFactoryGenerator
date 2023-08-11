namespace SimpleFactoryGenerator.Tests.Other;

public class ProductCrossAssemblyAttribute : ProductAttribute<string, IProductCrossAssembly>
{
    public ProductCrossAssemblyAttribute(string key) : base(key)
    {
    }
}
