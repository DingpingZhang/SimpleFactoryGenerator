using System.Collections.Generic;
using System.Linq;

namespace SimpleFactoryGenerator;

/// <summary>
/// An extension-method collection of the <see cref="IFactory{TKey, TProduct}"/>.
/// </summary>
public static class FactoryExtensions
{
    /// <summary>
    /// Gets a wrapper of the <see cref="IFactory{TKey, TProduct}"/> instance, which will cache the created product instances.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <returns>The wrapped factory instance.</returns>
    public static IFactory<TKey, TProduct> WithCache<TKey, TProduct>(this IFactory<TKey, TProduct> factory) where TProduct : class
    {
        return new CachedFactory<TKey, TProduct>(factory);
    }

    /// <summary>
    /// Determines if the specified <typeparamref name="TKey"/> instance can be used to create a product.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <returns>Returns a <see cref="bool"/> value to determine if the specified <typeparamref name="TKey"/> instance can be used to create a product.</returns>
    public static bool Contains<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.Creators.Any(creator => creator.CanCreate(key));
    }

    /// <summary>
    /// Creates all instances of the product that match the specified key.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <returns>Returns an iterable collection of products.</returns>
    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.Creators
            .Where(creator => creator.CanCreate(key))
            .Select(creator => creator.Create(key));
    }

    /// <summary>
    /// Creates the first product that match the specified key.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <returns>Returns the first product that match the specified key.</returns>
    /// <exception cref="System.InvalidOperationException" />
    public static TProduct CreateFirst<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.CreateAll(key).First();
    }

    /// <summary>
    /// Try creates the first product that match the specified key.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <param name="factory">The original factory instance.</param>
    /// <param name="key">The specified <typeparamref name="TKey"/> instance.</param>
    /// <param name="product">The first product that match the specified key.</param>
    /// <returns>Returns a <see cref="bool"/> value to determine if it succeeded.</returns>
    public static bool TryCreateFirst<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key, out TProduct product) where TProduct : class
    {
        TProduct? temp = factory.CreateAll(key).FirstOrDefault();
        product = temp!;
        return temp is not null;
    }

    private sealed class CachedFactory<TKey, TProduct> : IFactory<TKey, TProduct> where TProduct : class
    {
        public IReadOnlyCollection<ICreator<TKey, TProduct>> Creators { get; }

        public CachedFactory(IFactory<TKey, TProduct> factory)
        {
            Creators = factory.Creators.Select(creator => new CachedCreator(creator)).ToArray();
        }

        private sealed class CachedCreator : ICreator<TKey, TProduct>
        {
            private readonly ICreator<TKey, TProduct> _creator;

            private TProduct? _product;

            public CachedCreator(ICreator<TKey, TProduct> creator) => _creator = creator;

            public bool CanCreate(TKey key) => _creator.CanCreate(key);

            public TProduct Create(TKey key) => _product ??= _creator.Create(key);
        }
    }
}
