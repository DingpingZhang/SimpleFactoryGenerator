using System;

namespace SimpleFactoryGenerator.Tests.Other;

public class ProductCrossAssemblyAttribute : ProductAttribute<string, IProductCrossAssembly>
{
    public string Key { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Name { get; set; } = "10086";

    public int RetryCount { get; set; } = 3;

    public long Size { get; set; }

    public StringSplitOptions SplitOptions { get; set; } = StringSplitOptions.RemoveEmptyEntries;

    public ProductCrossAssemblyAttribute(string key, string when = "") : base(key)
    {
    }
}
