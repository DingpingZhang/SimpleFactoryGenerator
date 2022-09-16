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

public class TestCreatorAttribute : CreatorAttribute<ProductKey, IProduct>
{

}

public class Product1 : IProduct
{
    public Product1(string name)
    {
    }

    [TestCreator]
    private sealed class Creator : ICreator<ProductKey, IProduct>
    {
        public bool CanCreate(ProductKey key) => key.Name is "name1" or "name2";

        public IProduct Create(ProductKey key) => new Product1(key.Name);
    }
}

public class Product2 : IProduct
{
    [TestCreator]
    private sealed class Creator : ICreator<ProductKey, IProduct>
    {
        public bool CanCreate(ProductKey key) => key.Name is "type";

        public IProduct Create(ProductKey key) => new Product2();
    }
}

internal class Product3 : IProduct
{
    [TestCreator]
    internal sealed class Creator : ICreator<ProductKey, IProduct>
    {
        public bool CanCreate(ProductKey key) => string.IsNullOrEmpty(key.Name);

        public IProduct Create(ProductKey key) => new Product3();
    }
}
