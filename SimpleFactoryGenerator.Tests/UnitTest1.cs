using System;
using Xunit;

namespace SimpleFactoryGenerator.Tests
{
    public class ClassBase
    {

    }

    public class ClassAttribute : ProductOfSimpleFactoryAttribute<ClassBase, string>
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
            var factory = SimpleFactory
                .For<IProduct, ProductType>()
                .WithCache();
            IProduct product1 = factory.Create(ProductType.A);
            IProduct product2 = factory.Create(ProductType.B);
            IProduct product3 = factory.Create(ProductType.D);

            var f = SimpleFactory.For<ClassBase, string>();
            var p1 = f.Create(nameof(ProductOfClass1));
            var p2 = f.Create(nameof(ProductOfClass2));
        }

        [Product(ProductType.D)]
        private class Product4 : IProduct
        {
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

    public class ProductAttribute : ProductOfSimpleFactoryAttribute<IProduct, ProductType>
    {
        public ProductAttribute(ProductType key) : base(key)
        {
        }
    }

    //[ProductOfSimpleFactory<IProduct, string>(Consts.ProductName)]
    //[WorkRecord(Consts.ProductName)]
    //[ProductOfSimpleFactory<IProduct, ProductType>(ProductType.A)]
    [Product(ProductType.A)]
    internal class Product1 : IProduct
    {
    }

    //[ProductOfSimpleFactory<IProduct, ProductType>(ProductType.B)]
    //[ProductOfSimpleFactory<IProduct2, string>("2")]
    [Product(ProductType.B)]
    internal class Product2 : IProduct, IProduct2
    {
    }

    [Product(ProductType.C)]
    public class Product3 : IProduct
    {

    }

    public static class Consts
    {
        public const string ProductName = "5";
    }
}
