using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleFactoryGenerator.Tests.FactoryMethodPattern;

public class UnitTest
{
    [Fact]
    public void TestMethod()
    {
        var factory = Factory.For<ProductKey, IProduct>().WithCache();
        factory.CreateFirst(new ProductKey { Name = "name1" });
    }
}

public interface IProduct
{

}

public class ProductKey
{
    public string Name { get; set; } = null!;
}

public class TestProductAttribute<TCreator> : ProductAttribute<ProductKey, IProduct, TCreator>
    where TCreator : ICreator<ProductKey, IProduct>, new()
{

}

[TestProduct<Creator>]
public class Product1 : IProduct
{
    public Product1(string name)
    {
    }

    private sealed class Creator : ICreator<ProductKey, IProduct>
    {
        public bool CanCreate(ProductKey key) => key.Name is "name1" or "name2";

        public IProduct Create(ProductKey key) => new Product1(key.Name);
    }
}

[TestProduct<Creator>]
public class Product2 : IProduct
{
    private sealed class Creator : ICreator<ProductKey, IProduct>
    {
        public bool CanCreate(ProductKey key) => key.Name is "type";

        public IProduct Create(ProductKey key) => new Product2();
    }
}

[TestProduct<Creator>]
internal class Product3 : IProduct
{
    internal sealed class Creator : ICreator<ProductKey, IProduct>
    {
        public bool CanCreate(ProductKey key) => string.IsNullOrEmpty(key.Name);

        public IProduct Create(ProductKey key) => new Product3();
    }
}
