# SimpleFactoryGenerator [![version](https://img.shields.io/badge/nuget-0.4.0-orange)](https://www.nuget.org/packages/SimpleFactoryGenerator)

English | [中文](./README.zh-CN.md)

This library is used to assist in the implementation of the *Simple Factory Pattern* by automatically generating conditional branch structure in the factory class at **compile time**, thus solving the problem of the pattern violating the ["open-close principle"](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle).

## Why?

The introduction and use of the *Simple Factory Pattern* will not be described here. One of the drawbacks of this pattern is the need to manually maintain a (potentially huge) conditional branch structure for creating concrete instances of a given enumeration type (including strings). As a result, the pattern violates the "open to extensions, closed to modifications" design principle, which is solved by this library. The idea is very simple: design principles are a set of rules of thumb to facilitate the maintenance of code by *Humans*, to compensate for some of the limitations of human thinking. Therefore, there is no violation of the *Design Principles* by simply leaving this difficult-to-maintain part of the code to the compiler.

## How?

Let's start by looking at a simple factory implemented in the traditional way, in order to facilitate comparison of the parts replaced by this library.

```csharp
public interface IProduct
{
}

public class Product1 : IProduct
{
}

public class Product2 : IProduct
{
}

public class SimpleFactory
{
    public IProduct Create(string type)
    {
        // Here is the branch judgment that violates the open-close principle,
        // for example, when adding `Product3`, you need to manually add
        // a branch here (`"product_c" => new Product3(),`).
        return type switch
        {
            "product_a" => new Product1(),
            "product_b" => new Product2(),
            _ => throw new IndexOutOfRangeException(),
        }
    }
}

// Using
public static void Main()
{
    var factory = new SimpleFactory();
    IProduct product = factory.Create("product_a");
}
```

After using this library, the writing of `SimpleFactory` will be omitted and instead, a `ProductOfSimpleFactoryAttribute<T, K>` needs to be declared on the concrete `Product` type. You have already noticed: the Attribute uses generics, which requires [C# 11](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generics-and-attributes) support, for which you need to use Visual Studio 2022 and configure it in the `*.csproj` file: `<LangVersion>preview</LangVersion>`.

> If your project cannot be configured for C# 11 or uses Visual Studio 2022, which prevents you from **directly** using Generic-Attribute, you can refer to section 1.1 and customize an Attribute to inherit from `ProductOfSimpleFactoryAttribute<T, K>` (the Generic-Attribute definitions were allowed before C# 11, just not directly available).

```csharp
// It can also be an abstract class, or a normal class, and it is not mandatory to be an interface.
public interface IProduct
{
}

[ProductOfSimpleFactory<IProduct, string>("product_a")]
public class Product1 : IProduct
{
}

[ProductOfSimpleFactory<IProduct, string>("product_b")]
public class Product2 : IProduct
{
}

// Using
public static void Main()
{
    // The SimpleFactory static class are provided by this library.
    var factory = SimpleFactory
        .For<IProduct, string>()
        // .WithCache() is optional and when used will help implement the *Flyweight Pattern*
        // that caches created instances (i.e. instances with the same key are created multiple times
        // and the same instance is returned.)
        .WithCache();
    IProduct product = factory.Create("product_a");
}
```

## Advanced

It's not really that advanced, it's such a simple requirement, what do you expect from something advanced? :)

### 1.1 Custom Attribute

If you think the `ProductOfSimpleFactoryAttribute<T, K>` declaration too long, too ugly, too cumbersome (or can't use C# 11's Generic-Attribute syntax), you can customize an Attribute to inherit it.

```csharp
// Generic
public class ProductAttribute : ProductOfSimpleFactoryAttribute<IPorduct, string>
{
    public ProductAttribute(string productName) : base(productName)
    {
    }
}

// Non-generic
public class ProductAttribute : ProductOfSimpleFactoryAttribute
{
    public ProductAttribute(string productName) : base(productName)
    {
    }
}

[Product("product_a")]
public class Product1 : IProduct
{
}
```

### 1.2 Mapping to multiple target interfaces

For the same concrete product type, it is allowed to supply to multiple different target interfaces.

```csharp
[Product("product_a")]
[Mobile("iPhone")]
public class Product1 : IProduct, IMobile
{
}

// Using
public static void Main()
{
    var factory = SimpleFactory.For<IProduct, string>()；
    IProduct product = factory.Create("product_a");

    var factory = SimpleFactory.For<IMobile, string>()；
    IProduct mobile = factory.Create("iPhone");
}
```
