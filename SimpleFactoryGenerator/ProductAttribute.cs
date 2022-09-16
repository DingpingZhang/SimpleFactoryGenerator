using System;

namespace SimpleFactoryGenerator;

/// <summary>
/// Place this attribute onto a type to cause it to be considered a product of simple-factory pattern.
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ProductAttribute<TKey, TProduct> : Attribute where TProduct : class
{
    public ProductAttribute(TKey key)
    {
    }
}
