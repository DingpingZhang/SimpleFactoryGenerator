# SimpleFactoryGenerator [![version](https://img.shields.io/nuget/v/SimpleFactoryGenerator.svg)](https://www.nuget.org/packages/SimpleFactoryGenerator)

English | [中文](./README.zh-CN.md)

This library is used to assist in the implementation of the *Simple Factory Pattern* by automatically generating conditional branch structure in the factory class at **compile time**, thus solving the problem of the pattern violating the ["open-close principle"](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle).

Support for [Factory Method Pattern](https://refactoringguru.cn/design-patterns/factory-method) was added in `v0.6.0` (see section 3.1), so for the name of the repo *SimpleFactoryGenerator*, you can read as "Simple-Factory of Generator" or "Simple of Factory Generator".

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

### 3.1 Support for Factory Method Pattern

The simple factory pattern is limited to using a key to get a product, while in some more general cases we may need to use more complex conditions to get a product, or use a constructor with parameters to create a product, which requires some abstraction of the creation conditions and creation method.

The abstraction is actually "deferred implementation", leaving things that the library cannot decide to be implemented by the user. So the creation condition is abstracted into a method signed `CanCreate(TKey key): bool`, and the creation method is abstracted into a method signed `Create(TKey key): TProduct`; and both are included in an interface named `ICreator<TKey, TProduct>`.

Instead of focusing on the Product type and using `ProductAttribute`, we will use `CreatorAttribute` to mark the Creator used to create the Product, as follows.

```csharp
[Creator<Foo, IProduct>]
public class Product1Creator : ICreator<Foo, IProduct>
{
    public bool CanCreate(Foo foo) => foo.Name is "1" or "2" or "3";

    public IProduct Create(Foo foo) => new Product1(foo);
}

// Using: Replace SimpleFactory.For() with Factory.For().
var factory = Factory.For<Foo, IProduct>();
var key = new Foo("1");
// Note: CreateAll(), CreateFirst() and other methods are extension methods of `IFactory<K, U>` and need to be introduced into the `using SimpleFactoryGenerator;` namespace before they can be used.
IProduct product = factory.CreateAll(key).FirstOrDefault();
```

### 3.2 Custom Attribute

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

### 3.3 Mapping to multiple target interfaces

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
