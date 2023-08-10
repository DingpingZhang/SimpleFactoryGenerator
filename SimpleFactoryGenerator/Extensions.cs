using System.Collections.Concurrent;
using System.Collections;
using System;
using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public static class Extensions
{
    /// <summary>
    /// Creates all instances of the product in this simple-factory.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="args">The arguments required by the constructor that creates this instance.</param>
    /// <returns>Returns an iterable collection of products.</returns>
    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, params object?[] args)
    {
        foreach (var product in SimpleFactory<TKey, TProduct>.Products)
        {
            yield return factory.Create(product.Key, args);
        }
    }

    /// <summary>
    /// Determines if the specified <typeparamref name="TKey"/> instance can be used to create a product.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <returns>Returns a <see cref="bool"/> value to determine if the specified <typeparamref name="TKey"/> instance can be used to create a product.</returns>
    public static bool Contains<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key)
    {
        return SimpleFactory<TKey, TProduct>.Products.ContainsKey(key);
    }

    /// <summary>
    /// Try creates the product that match the specified key.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <param name="product">The product that match the specified key.</param>
    /// <param name="args">The arguments required by the constructor that creates this instance.</param>
    /// <returns>Returns a <see cref="bool"/> value to determine if it succeeded.</returns>
    public static bool TryCreate<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key, out TProduct product, params object?[] args)
    {
        if (factory.Contains(key))
        {
            product = factory.Create(key, args);
            return true;
        }

        product = default!;
        return false;
    }

    /// <summary>
    /// Gets a wrapper of the <see cref="ISimpleFactory{TKey, TProduct}"/> instance, which will cache the created product instances.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <returns>The wrapped factory instance.</returns>
    public static ISimpleFactory<TKey, TProduct> WithCache<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory)
    {
        return factory is CacheSimpleFactory<TKey, TProduct>
            ? factory
            : new CacheSimpleFactory<TKey, TProduct>(factory);
    }
}

sealed file class CacheSimpleFactory<TKey, TProduct> : ISimpleFactory<TKey, TProduct>
{
    private readonly ConcurrentDictionary<CacheKey, TProduct> _cache = new();
    private readonly ISimpleFactory<TKey, TProduct> _factory;

    public CacheSimpleFactory(ISimpleFactory<TKey, TProduct> factory) => _factory = factory;

    public ISimpleFactory<TKey, TProduct> WithCreator(Func<Type, object?[], TProduct> creator)
    {
        return _factory.WithCreator(creator);
    }

    public TProduct Create(TKey key, params object?[] args)
    {
        return _cache.GetOrAdd(new CacheKey(key, args), CreateInstance);

        TProduct CreateInstance(CacheKey x) => _factory.Create(x.Key, x.Args);
    }

    private class CacheKey
    {
        public TKey Key { get; }

        public object?[] Args { get; }

        public CacheKey(TKey key, object?[] args)
        {
            Key = key;
            Args = args;
        }

        public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();

        public override int GetHashCode()
        {
            int keyCode = Key?.GetHashCode() ?? 0;
            int argsCode = ((IStructuralEquatable)Args).GetHashCode(EqualityComparer<object?>.Default);

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + keyCode;
                hash = hash * 31 + argsCode;
                return hash;
            }
        }
    }
}
