using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFactoryGenerator;

/// <summary>
/// An extension-method collection of the <see cref="ISimpleFactory{TKey, TProduct}"/>.
/// </summary>
public static class SimpleFactoryExtensions
{
    /// <summary>
    /// Gets a wrapper of the <see cref="ISimpleFactory{TKey, TProduct}"/> instance, which will cache the created product instances.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <returns>The wrapped factory instance.</returns>
    public static ISimpleFactory<TKey, TProduct> WithCache<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory) where TProduct : class
    {
        return new CachedSimpleFactory<TKey, TProduct>(factory);
    }

    /// <summary>
    /// Determines if the specified <typeparamref name="TKey"/> instance can be used to create a product.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <returns>Returns a <see cref="bool"/> value to determine if the specified <typeparamref name="TKey"/> instance can be used to create a product.</returns>
    public static bool Contains<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.Keys.Any(item => EqualityComparer<TKey>.Default.Equals(key, item));
    }

    /// <summary>
    /// Try creates the product that match the specified key.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <param name="product">The product that match the specified key.</param>
    /// <returns>Returns a <see cref="bool"/> value to determine if it succeeded.</returns>
    public static bool TryCreate<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key, out TProduct product) where TProduct : class
    {
        bool contains = factory.Contains(key);
        product = contains ? factory.Create(key) : default!;
        return contains;
    }

    /// <summary>
    /// Creates all instances of the product in this simple-factory.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <returns>Returns an iterable collection of products.</returns>
    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory) where TProduct : class
    {
        return factory.Keys.Select(factory.Create);
    }

    private sealed class CachedSimpleFactory<TKey, TProduct> : ISimpleFactory<TKey, TProduct> where TProduct : class
    {
        private readonly ConcurrentDictionary<TKey, TProduct> _cache = new();
        private readonly ISimpleFactory<TKey, TProduct> _factory;

        public CachedSimpleFactory(ISimpleFactory<TKey, TProduct> factory) => _factory = factory;

        public IReadOnlyCollection<TKey> Keys => _factory.Keys;

        public TProduct Create(TKey feed) => _cache.GetOrAdd(feed, _factory.Create);
    }
}
