using System;
using Xunit;

namespace SimpleFactoryGenerator.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            ISimpleFactory<IProduct, string> factory = SimpleFactory
                .For<IProduct, string>()
                .WithCache();
            IProduct product1 = factory.Create(Consts.ProductName);
            IProduct product2 = factory.Create("1");
        }
    }

    public interface IProduct
    {
    }

    public interface IProduct2
    {

    }

    public class WorkRecordAttribute : ProductOfSimpleFactoryAttribute<IProduct, string>
    {
        public WorkRecordAttribute(string key) : base(key)
        {
        }
    }

    //[ProductOfSimpleFactory<IProduct, string>(Consts.ProductName)]
    [WorkRecord(Consts.ProductName)]
    internal class Product1 : IProduct
    {
    }

    [ProductOfSimpleFactory<IProduct, string>("12")]
    [ProductOfSimpleFactory<IProduct2, string>("2")]
    internal class Product2 : IProduct, IProduct2
    {
    }

    public static class Consts
    {
        public const string ProductName = "5";
    }
}
