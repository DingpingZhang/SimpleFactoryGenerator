namespace SimpleFactoryGenerator.Tests.Other;

public class ProductCrossAssemblyAttribute : ProductAttribute<string, IProductCrossAssembly>
{
    public string Key { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ProductCrossAssemblyAttribute(string key, string when = "") : base(key)
    {
    }
}
