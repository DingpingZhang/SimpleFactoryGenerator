using SimpleFactoryGenerator.Tests.Other;

namespace SimpleFactoryGenerator.Tests;

[ProductCrossAssembly(key: nameof(Product1InThisAssembly), when: "text.dirty", Key = "ctrl+s", Description = "保存文件")]
[ProductCrossAssembly("shell.saveAll", when: "any(text.dirty)", Key = "ctrl+shift+s", Description = "保存所有文件")]
internal class Product1InThisAssembly : IProductCrossAssembly
{
    public string Name { get; }

    public Product1InThisAssembly(string name) => Name = name;
}
