using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using Xunit;

namespace SimpleFactoryGenerator.Tests;

public class ClassBase
{

}

public class ClassAttribute : ProductAttribute<string, ClassBase>
{
    public ClassAttribute(string key) : base(key)
    {
    }
}

[Class(nameof(ProductOfClass1))]
public class ProductOfClass1 : ClassBase
{

}

[Class(nameof(ProductOfClass2))]
public class ProductOfClass2 : ClassBase
{

}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var products = SimpleFactory<ProductType, IProduct>.Products;

        var factory1 = SimpleFactory.For<ProductType, IProduct>()
            //.UseCreator((type, args) => null!)
            .UseCache()
            .Build<string>();
        var factory2 = SimpleFactory
            .For<ProductType, IProduct>()
            .UseCache();

        var arr = factory1.CreateAll("111111").ToArray();

        var a1 = factory1.Create(ProductType.A, "1");
        var a2 = factory1.Create(ProductType.B, "11");
        var a3 = factory1.Create(ProductType.C, "111");
        var a4 = factory1.Create(ProductType.D, "1111");
        var a5 = factory1.Create(ProductType.D, "11111");
        var a6 = factory1.Create(ProductType.D, "111111");
        var a7 = factory1.Create(ProductType.D, "111111");
        var a8 = factory1.Create(ProductType.D, "111111");
    }

    [Product(ProductType.D)]
    private class Product4 : IProduct
    {
        public Product4(string a)
        {
        }
    }
}

public interface IProduct
{
}

public interface IProduct2
{

}

public enum ProductType : byte
{
    A,
    B,
    C,
    D,
}

public class ProductAttribute : ProductAttribute<ProductType, IProduct>
{
    public ProductAttribute(ProductType key) : base(key)
    {
    }
}

public class PAttribute : ProductAttribute<string, IProduct>
{
    public PAttribute(string key) : base(key)
    {
    }
}

//[Product<IProduct, string>(Consts.ProductName)]
//[WorkRecord(Consts.ProductName)]
//[Product<IProduct, ProductType>(ProductType.A)]
[Product(ProductType.A)]
internal class Product1 : IProduct
{
    public Product1(string a)
    {
    }
}

//[Product<IProduct, ProductType>(ProductType.B)]
//[Product<IProduct2, string>("2")]
[Product(ProductType.B)]
[Product(ProductType.C)]
internal class Product2 : IProduct, IProduct2
{
    public Product2(string a)
    {
    }
}

//[Product(ProductType.C)]
//public class Product3 : IProduct
//{

//}

public static class Consts
{
    public const string ProductName = "5";
}
