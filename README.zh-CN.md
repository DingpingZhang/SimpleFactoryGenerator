﻿# SimpleFactoryGenerator [![version](https://img.shields.io/nuget/v/SimpleFactoryGenerator.svg)](https://www.nuget.org/packages/SimpleFactoryGenerator)

[English](./README.md) | 中文

本库用于辅助简单工厂模式（Simple Factory Pattern）的实现，即在编译时自动生成简单工厂中的条件分支语句（`switch-case` 或 `if-else`），从而解决该模式违背“开闭原则”（[The Open/Closed Principle](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle)）的问题。

## 1. 为什么使用？

关于简单工厂模式（[factory-comparison](https://refactoringguru.cn/design-patterns/factory-comparison)）的介绍与使用，此处不予赘述。该模式的一个缺点是：需要维护一个（可能十分巨大）的条件分支结构，用于根据给定的枚举类型创建具体的实例。因此，该模式违背了“对扩展开放，对修改封闭”的设计原则，本仓库解决了这个问题。思路非常简单：设计原则只是为了便于**人类**维护代码的一套经验规则，用于弥补人类思维的一些局限性，那么，只需要将这部分难以维护的代码交给编译器维护，就不存在违背“设计原则”的说法了。

## 2. 如何使用？

我们先来看一个传统方式实现的简单工厂，以便于对比本仓库所代替的部分：

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
        // NOTE: 此处便是违背了开闭原则的分支判断，例：当加入 Product3 后，
        // 需要手动在此添加一个分支 `"product_c" => new Product3(),`
        return type switch
        {
            "product_a" => new Product1(),
            "product_b" => new Product2(),
            _ => throw new IndexOutOfRangeException(),
        }
    }
}

// 使用

var factory = new SimpleFactory();
IProduct product = factory.Create("product_a");
```

在使用本仓库后，将省去 `SimpleFactory` 的编写，而代替的，需要在具体的 `Product` 类型上声明一个 `ProductAttribute<K, T>`。你已经注意到：该 Attribute 使用了泛型，这需要 [C# 11](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generics-and-attributes) 的支持，为此，你需要使用 VS2022，并在 `*.csproj` 文件中配置：`<LangVersion>preview</LangVersion>`。

> 如果你的项目无法配置 C# 11 或使用 VS 2022，导致无法**直接**使用 Generic-Attribute，可以参考 3.1 小节，自定义一个 Attribute 继承自 `ProductAttribute<K, T>` 即可（在 C# 11 之前允许定义泛型 Attribute，只是无法直接使用而已）。

```csharp
// 也可以是 abstract class，或普通的 class，不强制要求是 interface。
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

// 使用

// SimpleFactory 静态类由本仓库提供。
var factory = SimpleFactory
    .For<string, IProduct>()
    // .WithCache() 是可选的一个装饰器，其缓存已创建的实例（即多次创建 key 相同的实例，将返回同一个实例。）
    .WithCache();
IProduct product = factory.Create("product_a");
```

## 3. 进阶使用

其实没有多进阶，就这么简单的一个需求，你还指望有什么高级的东西？也就是使用本库的一些小技巧。

### 3.1 自定义 Attribute

如果你觉得 `ProductAttribute<K, T>` 声明太长、太丑、太麻烦（或者无法使用 C# 11 的泛型 Attribute 语法），可以自定义一个 Attribute 继承它，也是生效的，如：

```csharp
public class FruitAttribute : ProductAttribute<string, IProduct>
{
    public FruitAttribute(string name) : base(name)
    {
    }
}

[Fruit("apple")]
public class Apple : IProduct
{
}
```

### 3.2 映射到多个目标接口

对于同一个具体产品类型，是允许供应给多个不同的目标接口的，如：

```csharp
[Animal("老鼠")]
[Food("鸭脖")]
public class Mouse : IAnimal, IFood
{
}

// Using
var animalFactory = SimpleFactory.For<string, IAnimal>();
IProduct mouse = animalFactory.Create("老鼠");

var foodFactory = SimpleFactory.For<string, IFood>();
IProduct duckNeck = foodFactory.Create("鸭脖");
```

### 3.3 传递参数到构造函数

若 `Product` 类型的构造函数具有参数，则可以按以下方式进行传递：

```csharp
_ = factory.Create("key", arg1, arg2, ...);
```

由于使用 `factory` 时，往往不能够确定创建类型的构造函数，因而建议所有 `Product` 构造函数的参数列表应该一致。

若有需求使得各个 `Product` 的构造函数参数不确定，则建议通过 Ioc 容器来创建：

```csharp
var factory = SimpleFactory
    .For<Key, Product>()
    .WithCreator((type, args) => (Product)container.Resolve(type, args));

_ = factory.Create(key);
```

注意：若在 `.WithCache()` 之后使用 `.WithCreator()`，则会导致之前的缓存被清空。（这很合理，一朝天子一朝臣，Creator 都变了，哪里还敢用之前的缓存）
