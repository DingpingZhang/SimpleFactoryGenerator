# SimpleFactoryGenerator [![version](https://img.shields.io/nuget/v/SimpleFactoryGenerator.svg)](https://www.nuget.org/packages/SimpleFactoryGenerator)

English | [中文](./README.zh-CN.md)

This library is used to assist in the implementation of the *Simple Factory Pattern* by automatically generating conditional branch structure in the factory class at **compile time**, thus solving the problem of the pattern violating the ["open-close principle"](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle).

## 1. Why?

The introduction and use of the *Simple Factory Pattern* will not be described here. One of the drawbacks of this pattern is the need to manually maintain a (potentially huge) conditional branch structure for creating concrete instances of a given enumeration type (including strings). As a result, the pattern violates the "open to extensions, closed to modifications" design principle, which is solved by this library. The idea is very simple: design principles are a set of rules of thumb to facilitate the maintenance of code by *Humans*, to compensate for some of the limitations of human thinking. Therefore, there is no violation of the *Design Principles* by simply leaving this difficult-to-maintain part of the code to the **compiler**.

## 2. How?

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

var factory = new SimpleFactory();
IProduct product = factory.Create("product_a");
```

After using this library, the writing of `SimpleFactory` will be omitted and instead, a `ProductAttribute<K, T>` needs to be declared on the concrete `Product` type. You have already noticed: the Attribute uses generics, which requires [C# 11](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generics-and-attributes) support, for which you need to use Visual Studio 2022 and configure it in the `*.csproj` file: `<LangVersion>preview</LangVersion>`.

> If your project cannot be configured for C# 11 or uses Visual Studio 2022, which prevents you from **directly** using Generic-Attribute, you can refer to section 1.1 and customize an Attribute to inherit from `ProductAttribute<K, T>` (the Generic-Attribute definitions were allowed before C# 11, just not directly available).

```csharp
// It can also be an abstract class, or a normal class, and it is not mandatory to be an interface.
public interface IProduct
{
}

[Product<string, IProduct>("product_a")]
public class Product1 : IProduct
{
}

[Product<string, IProduct>("product_b")]
public class Product2 : IProduct
{
}

// Using

// The SimpleFactory static class are provided by this library.
var factory = SimpleFactory
    .For<string, IProduct>()
    // .WithCache() is optional and when used will help implement the *Flyweight Pattern*
    // that caches created instances (i.e. instances with the same key are created multiple times
    // and the same instance is returned.)
    .WithCache();
IProduct product = factory.Create("product_a");
```

## 3. Advanced

It's not really that advanced, it's such a simple requirement, what are you expecting? :)

### 3.1 Custom Attribute

If you think the `ProductAttribute<K, T>` declaration too long, too ugly, too cumbersome (or can't use C# 11's Generic-Attribute syntax), you can customize an Attribute to inherit it.

```csharp
public class ProductAttribute : ProductAttribute<string, IProduct>
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

### 3.2 Mapping to multiple target interfaces

For the same concrete product type, it is allowed to supply to multiple different target interfaces.

```csharp
[Product("product_a")]
[Mobile("iPhone")]
public class Product1 : IProduct, IMobile
{
}

// Using
var factory = SimpleFactory.For<string, IProduct>();
IProduct product = factory.Create("product_a");
var factory = SimpleFactory.For<string, IMobile>();
IProduct mobile = factory.Create("iPhone");
```

### 3.3 Passing arguments to the constructor

If a constructor of type `Product` has parameters, they can be passed as follows:

```csharp
_ = factory.Create("key", arg1, arg2, ...);
```

Since it is often not possible to determine the constructor that creates the type when using `factory`, it is recommended that all `Product` constructors have a consistent argument list.

If there is a need to make the constructor arguments for each `Product` indeterminate, it is recommended that it be created in an Ioc container:

```csharp
var factory = SimpleFactory
    .For<Key, Product>()
    .WithCreator((type, args) => container.Resolve(type, args));

_ = factory.Create(key);
```
