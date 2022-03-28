# SimpleFactoryGenerator [![version](https://img.shields.io/badge/nuget-0.2.0-orange)](https://www.nuget.org/packages/SimpleFactoryGenerator)

本库用于辅助简单工厂模式（Simple Factory Pattern）的实现，即在编译时自动生成简单工厂中的条件分支语句（`switch-case` 或 `if-else`），从而解决该模式违背“开闭原则”（[The Open/Closed Principle](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle)）的问题。

## 为什么使用？

关于简单工厂模式（[factory-comparison](https://refactoringguru.cn/design-patterns/factory-comparison)）的介绍与使用，此处不予赘述。该模式的一个缺点是：需要维护一个（可能十分巨大）的条件分支结构，用于根据给定的枚举类型创建具体的实例。因此，该模式违背了“对扩展开放，对修改封闭”的设计原则，本仓库解决了这个问题。思路非常简单：设计原则只是为了便于**人类**维护代码的一套经验规则，用于弥补人类思维的一些局限性，那么，只需要将这部分难以维护的代码交给编译器维护，就不存在违背“设计原则”的说法了。

## 如何使用？

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
public static void Main()
{
    var factory = new SimpleFactory();
    IProduct product = factory.Create("product_a");
}
```

在使用本仓库后，将省去 `SimpleFactory` 的编写，而代替地，需要在具体的 Product 类型上声明一个 `ProductOfSimpleFactoryAttribute<T, K>`。你已经注意到：该 Attribute 使用了泛型，这需要 [C# 11](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generics-and-attributes) 的支持，为此，你需要使用 VS2022，并在 `*.csproj` 文件中配置：`<LangVersion>preview</LangVersion>`。

~~虽然也可以实现非泛型的版本以兼容 Legacy Code，但那种语法长得太丑了，反正我自己的项目已经满足这一条件了，为什么不抛弃旧时代呢？~~

```csharp
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

// 使用
public static void Main()
{
    // SimpleFactory 静态类由本仓库提供。
    var factory = SimpleFactory
        .For<IProduct, string>()
        // .WithCache() 是可选的，使用后将帮助实现“享元模式”，缓存已创建的实例（即多次创建 key 相同的实例，将返回同一个实例。）
        .WithCache();
    IProduct product = factory.Create("product_a");
}
```

## 高级使用

其实没有多高级，就这么简单的一个需求，你还指望有什么高级的东西？

### 1.1 自定义 Attribute

如果你觉得 `ProductOfSimpleFactoryAttribute<T, K>` 声明太长、太丑、太麻烦，可以自定义一个 Attribute 继承它，也是生效的，如：

```csharp
public class ProductAttribute : ProductOfSimpleFactoryAttribute<IPorduct, string>
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

### 1.2 映射到多个目标接口

对于同一个具体产品类型，是允许供应给多个不同的目标接口的，如：

```csharp
[Product("product_a")]
[Mobile("iPhone")]
public class Product1 : IProduct, IMobile
{
}

// 使用
public static void Main()
{
    var factory = SimpleFactory.For<IProduct, string>()；
    IProduct product = factory.Create("product_a");

    var factory = SimpleFactory.For<IMobile, string>()；
    IProduct mobile = factory.Create("iPhone");
}
```
