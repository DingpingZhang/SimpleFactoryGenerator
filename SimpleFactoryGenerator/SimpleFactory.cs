using System;
using System.Collections.Concurrent;
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
        return new SimpleFactory<TKey, TProduct>(SimpleFactory<TKey, TProduct>.Products);
    }
}

/// <summary>
/// Represents a simple-factory implementation.
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
public class SimpleFactory<TKey, TProduct> : ISimpleFactory<TKey, TProduct>
{
    private static readonly ConcurrentDictionary<TKey, Type> Storage = new();

    public static IReadOnlyDictionary<TKey, Type> Products => Storage;

    public static void Register(TKey key, Type type) => Storage.TryAdd(key, type);

    private static TProduct DefaultCreator(Type type, object?[] args) => (TProduct)Activator.CreateInstance(type, args);

    private readonly IReadOnlyDictionary<TKey, Type> _collector;

    private Func<Type, object?[], TProduct> _creator = DefaultCreator;

    internal SimpleFactory(IReadOnlyDictionary<TKey, Type> collector) => _collector = collector;

    /// <inheritdoc />
    public ISimpleFactory<TKey, TProduct> WithCreator(Func<Type, object?[], TProduct> creator)
    {
        _creator = creator;
        return this;
    }

    /// <inheritdoc />
    public TProduct Create(TKey key, params object?[] args) => _creator(_collector[key], args);
}
