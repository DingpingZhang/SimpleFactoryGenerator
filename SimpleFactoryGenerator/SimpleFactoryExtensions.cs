using System.Collections.Concurrent;

namespace SimpleFactoryGenerator
{
    public static class SimpleFactoryExtensions
    {
        public static ISimpleFactory<TTarget, TKey> WithCache<TTarget, TKey>(this ISimpleFactory<TTarget, TKey> factory)
        {
            return new CachedSimpleFactory<TTarget, TKey>(factory);
        }

        private class CachedSimpleFactory<TTarget, TKey> : ISimpleFactory<TTarget, TKey>
        {
            private readonly ConcurrentDictionary<TKey, TTarget> _cache = new();
            private readonly ISimpleFactory<TTarget, TKey> _source;

            public CachedSimpleFactory(ISimpleFactory<TTarget, TKey> source) => _source = source;

            public TTarget Create(TKey key) => _cache.GetOrAdd(key, k => _source.Create(k));
        }
    }
}
