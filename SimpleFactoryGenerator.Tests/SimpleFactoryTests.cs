using System;
using System.Linq;
using Shouldly;
using SimpleFactoryGenerator.Tests.Other;
using Xunit;

namespace SimpleFactoryGenerator.Tests;

public class SimpleFactoryTests
{
    [Fact(DisplayName = "It should support product classes across assemblies.")]
    public void SupportCrossAssembly()
    {
        var products = SimpleFactory<string, IProductCrossAssembly>.Products;

        products.Count.ShouldBe(3);
        products.Values.Contains(typeof(Product1InThisAssembly)).ShouldBeTrue();

        var product = products["Product1InOtherAssembly"];
        product.Assembly.GetName().Name!.ShouldBe("SimpleFactoryGenerator.Tests.Other");
    }

    [Fact(DisplayName = "it should replace the same key with the type registered later.")]
    public void ReplaceWithTypeRegisteredLater()
    {
        var products = SimpleFactory<string, IProductCrossAssembly>.Products;
        var product = products[nameof(Product2InOtherAssembly)];

        product.ShouldBe(typeof(Product2InOtherAssembly));
        product.Assembly.GetName().Name!.ShouldNotBe("SimpleFactoryGenerator.Tests.Other");
    }

    [Fact(DisplayName = "it should be create types with parameters.")]
    public void CreateWithParameters()
    {
        var factory = SimpleFactory.For<string, IProductCrossAssembly>();
        var products = SimpleFactory<string, IProductCrossAssembly>.Products;

        foreach (var info in products)
        {
            string name = Guid.NewGuid().ToString();
            bool success = factory.TryCreate(info.Key, out var product, name);

            success.ShouldBeTrue();
            product.GetType().ShouldBe(info.Value);
            product.Name.ShouldBe(name);
        }
    }

    [Fact(DisplayName = "it should support caching.")]
    public void SupportCache()
    {
        int count = 0;
        var factory = SimpleFactory.For<string, IProductCrossAssembly>()
            .WithCreator((type, args) =>
            {
                count++;
                return (IProductCrossAssembly)Activator.CreateInstance(type, args)!;
            })
            .WithCache();
        var products = SimpleFactory<string, IProductCrossAssembly>.Products;

        count.ShouldBe(0);
        string name = Guid.NewGuid().ToString();
        _ = factory.CreateAll(name).ToArray();
        count.ShouldBe(products.Count);

        count = 0;
        _ = factory.CreateAll(name).ToArray();
        count.ShouldBe(0);
    }

    [Fact(DisplayName = "it should clear the cache when the creator is updated.")]
    public void ClearCache()
    {
        var factory = SimpleFactory.For<string, IProductCrossAssembly>().WithCache();
        var products = SimpleFactory<string, IProductCrossAssembly>.Products;

        string name = Guid.NewGuid().ToString();
        _ = factory.CreateAll(name).ToArray();

        int count = 0;
        factory = factory.WithCreator((_, _) =>
        {
            count++;
            return null!;
        });
        count = 0;
        var instances = factory.CreateAll(name).ToArray();
        count.ShouldBe(products.Count);
        instances.ShouldAllBe(x => x == null!);
    }
}
