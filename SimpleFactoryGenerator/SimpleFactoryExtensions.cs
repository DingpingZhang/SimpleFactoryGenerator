using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFactoryGenerator;

public static class SimpleFactoryExtensions
{
    public static ISimpleFactory<TTarget, TKey> WithCache<TTarget, TKey>(this ISimpleFactory<TTarget, TKey> factory) where TTarget : class
    {
        return new CachedSimpleFactory<TTarget, TKey>(factory);
    }

    public static bool Contains<TTarget, TKey>(this ISimpleFactory<TTarget, TKey> factory, TKey key) where TTarget : class
    {
        return factory.Keys.Any(item => EqualityComparer<TKey>.Default.Equals(key, item));
    }

    public static bool TryCreate<TTarget, TKey>(this ISimpleFactory<TTarget, TKey> factory, TKey key, out TTarget result) where TTarget : class
    {
        bool contains = factory.Contains(key);
        result = contains ? factory.Create(key) : null!;
        return contains;
    }

    public static IEnumerable<TTarget> CreateAll<TTarget, TKey>(this ISimpleFactory<TTarget, TKey> factory) where TTarget : class
    {
        return factory.Keys.Select(factory.Create);
    }

    private class CachedSimpleFactory<TTarget, TKey> : ISimpleFactory<TTarget, TKey> where TTarget : class
    {
        private readonly ConcurrentDictionary<TKey, TTarget> _cache = new();
        private readonly ISimpleFactory<TTarget, TKey> _source;

        public CachedSimpleFactory(ISimpleFactory<TTarget, TKey> source) => _source = source;

        public IReadOnlyCollection<TKey> Keys => _source.Keys;

        public TTarget Create(TKey key) => _cache.GetOrAdd(key, _source.Create);
    }
}
