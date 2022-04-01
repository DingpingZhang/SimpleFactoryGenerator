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
            IProduct product3 = factory.Create(ProductType.D);
        }


        [ProductOfSimpleFactory(typeof(IProduct), typeof(ProductType), ProductType.D)]
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

    public class WorkRecordAttribute : ProductOfSimpleFactoryAttribute<IProduct, string>
    {
        public WorkRecordAttribute(string key) : base(key)
        {
        }
    }

    //[ProductOfSimpleFactory<IProduct, string>(Consts.ProductName)]
    //[WorkRecord(Consts.ProductName)]
    //[ProductOfSimpleFactory<IProduct, ProductType>(ProductType.A)]
    [ProductOfSimpleFactory(typeof(IProduct), typeof(ProductType), ProductType.A)]
    internal class Product1 : IProduct
    {
    }

    //[ProductOfSimpleFactory<IProduct, ProductType>(ProductType.B)]
    //[ProductOfSimpleFactory<IProduct2, string>("2")]
    [ProductOfSimpleFactory(typeof(IProduct), typeof(ProductType), ProductType.B)]
    internal class Product2 : IProduct, IProduct2
    {
    }

    [ProductOfSimpleFactory(typeof(IProduct), typeof(ProductType), ProductType.C)]
    public class Product3 : IProduct
    {

    }

    public static class Consts
    {
        public const string ProductName = "5";
    }
}
