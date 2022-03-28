using System;
using Xunit;

namespace SimpleFactoryGenerator.Tests
{
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
    }

    public class WorkRecordAttribute : ProductOfSimpleFactoryAttribute<IProduct, string>
    {
        public WorkRecordAttribute(string key) : base(key)
        {
        }
    }

    //[ProductOfSimpleFactory<IProduct, string>(Consts.ProductName)]
    //[WorkRecord(Consts.ProductName)]
    [ProductOfSimpleFactory<IProduct, ProductType>(ProductType.A)]
    internal class Product1 : IProduct
    {
    }

    [ProductOfSimpleFactory<IProduct, ProductType>(ProductType.B)]
    [ProductOfSimpleFactory<IProduct2, string>("2")]
    internal class Product2 : IProduct, IProduct2
    {
    }

    public static class Consts
    {
        public const string ProductName = "5";
    }
}
