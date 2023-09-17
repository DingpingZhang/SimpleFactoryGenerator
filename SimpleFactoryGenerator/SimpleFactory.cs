using System;
using System.Collections.Generic;

namespace SimpleFactoryGenerator;

/// <summary>
/// Represents the builder of simple-factory.
/// </summary>
public static class SimpleFactory
{
    /// <summary>
    /// Gets a simple-factory by the specified <typeparamref name="TKey"/> and <typeparamref name="TProduct"/> type.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <returns>Returns a simple-factory used to create the <typeparamref name="TProduct"/> instance by the specified <typeparamref name="TKey"/>.</returns>
    public static ISimpleFactory<TKey, TProduct> For<TKey, TProduct>()
    {
        return new SimpleFactory<TKey, TProduct>();
    }
}

/// <summary>
/// Represents a simple-factory implementation.
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
public class SimpleFactory<TKey, TProduct> : ISimpleFactory<TKey, TProduct>
{
    private static readonly Dictionary<TKey, Type> ProductStorage = new();
    private static readonly Dictionary<TKey, ITags> TagStorage = new();

    public static IReadOnlyDictionary<TKey, Type> Products => ProductStorage;

    public static IReadOnlyDictionary<TKey, ITags> Tags => TagStorage;

    public static void Register(TKey key, Type type, ITags tags)
    {
        ProductStorage[key] = type;
        TagStorage[key] = tags;
    }

    private static TProduct DefaultCreator(TKey key, Type type, object?[] args, ITags tags)
    {
        return (TProduct)Activator.CreateInstance(type, args);
    }

    private ProductCreator<TKey, TProduct> _creator = DefaultCreator;

    internal SimpleFactory() { }

    /// <inheritdoc />
    public ISimpleFactory<TKey, TProduct> WithCreator(ProductCreator<TKey, TProduct> creator)
    {
        _creator = creator;
        return this;
    }

    /// <inheritdoc />
    public TProduct Create(TKey key, params object?[] args)
    {
        return _creator(key, Products[key], args, Tags[key]);
    }
}
