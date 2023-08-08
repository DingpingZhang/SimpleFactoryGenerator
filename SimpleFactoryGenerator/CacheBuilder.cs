using System;
using System.Collections.Concurrent;

namespace SimpleFactoryGenerator;

internal partial class CacheBuilder<TKey, TProduct> : ISimpleFactoryBuilder<TKey, TProduct>
{
    private readonly ConcurrentDictionary<ICacheKey<TKey>, TProduct> _cache = new();
    private readonly ISimpleFactoryBuilder<TKey, TProduct> _builder;

    public CacheBuilder(ISimpleFactoryBuilder<TKey, TProduct> builder) => _builder = builder;

    public ISimpleFactoryBuilder<TKey, TProduct> UseCreator(Func<Type, object?[], TProduct> creator)
    {
        return _builder.UseCreator(creator);
    }

    public ISimpleFactoryBuilder<TKey, TProduct> UseCache() => this;

    public ISimpleFactory<TKey, TProduct> Build()
    {
        var factory = _builder.Build();
        return new SimpleFactoryImpl<TKey, TProduct>(key => _cache.GetOrAdd(new CacheKey<TKey>(key), Create));
        TProduct Create(ICacheKey<TKey> cacheKey) => factory.Create(cacheKey.Key);
    }
}

readonly file struct CacheKey<TKey> : ICacheKey<TKey>
{
    public TKey Key { get; }

    public CacheKey(TKey key) => Key = key;
}
