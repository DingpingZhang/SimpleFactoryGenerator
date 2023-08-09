using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleFactoryGenerator;

internal class CacheSimpleFactory<TKey, TProduct> : ISimpleFactory<TKey, TProduct>
{
    private readonly ConcurrentDictionary<CacheKey, TProduct> _cache = new();
    private readonly ISimpleFactory<TKey, TProduct> _factory;

    public CacheSimpleFactory(ISimpleFactory<TKey, TProduct> factory) => _factory = factory;

    public ISimpleFactory<TKey, TProduct> WithCreator(Func<Type, object?[], TProduct> creator)
    {
        return _factory.WithCreator(creator);
    }

    public ISimpleFactory<TKey, TProduct> WithCache() => this;

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
