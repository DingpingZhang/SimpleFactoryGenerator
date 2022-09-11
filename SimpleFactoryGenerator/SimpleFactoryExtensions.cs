using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFactoryGenerator;

public static class SimpleFactoryExtensions
{
    public static ISimpleFactory<TKey, TProduct> WithCache<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory) where TProduct : class
    {
        return new CachedSimpleFactory<TKey, TProduct>(factory);
    }

    public static bool Contains<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.Keys.Any(item => EqualityComparer<TKey>.Default.Equals(key, item));
    }

    public static bool TryCreate<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key, out TProduct result) where TProduct : class
    {
        bool contains = factory.Contains(key);
        result = contains ? factory.Create(key) : null!;
        return contains;
    }

    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory) where TProduct : class
    {
        return factory.Keys.Select(factory.Create);
    }

    private class CachedSimpleFactory<TKey, TProduct> : ISimpleFactory<TKey, TProduct> where TProduct : class
    {
        private readonly ConcurrentDictionary<TKey, TProduct> _cache = new();
        private readonly ISimpleFactory<TKey, TProduct> _source;

        public CachedSimpleFactory(ISimpleFactory<TKey, TProduct> source) => _source = source;

        public IReadOnlyCollection<TKey> Keys => _source.Keys;

        public TProduct Create(TKey feed) => _cache.GetOrAdd(feed, _source.Create);
    }
}
