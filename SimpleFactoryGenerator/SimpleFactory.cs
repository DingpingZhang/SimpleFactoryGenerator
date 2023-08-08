using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public static class SimpleFactory
{
    public static ISimpleFactoryBuilder<TKey, TProduct> For<TKey, TProduct>()
    {
        return new Builder<TKey, TProduct>(SimpleFactory<TKey, TProduct>.Products);
    }

    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(
        this ISimpleFactoryBuilder<TKey, TProduct> builder,
        Func<ISimpleFactoryBuilder<TKey, TProduct>, Func<TKey, TProduct>> creator)
    {
        var build = creator(builder);
        foreach (var product in SimpleFactory<TKey, TProduct>.Products)
        {
            yield return build(product.Key);
        }
    }
}

public static class SimpleFactory<TKey, TProduct>
{
    private static readonly ConcurrentDictionary<TKey, Type> Storage = new();

    public static IReadOnlyDictionary<TKey, Type> Products => Storage;

    public static void Register(TKey key, Type type) => Storage.TryAdd(key, type);
}

sealed file class Builder<TKey, TProduct> : ISimpleFactoryBuilder<TKey, TProduct>
{
    private readonly IReadOnlyDictionary<TKey, Type> _collector;

    private Func<Type, object?[], TProduct> _creator = (type, args) => (TProduct)Activator.CreateInstance(type, args);

    public Builder(IReadOnlyDictionary<TKey, Type> collector) => _collector = collector;

    public ISimpleFactoryBuilder<TKey, TProduct> UseCreator(Func<Type, object?[], TProduct> creator)
    {
        _creator = creator;
        return this;
    }

    public ISimpleFactoryBuilder<TKey, TProduct> UseCache()
    {
        return new CacheBuilder<TKey, TProduct>(this);
    }

    public Func<TKey, TProduct> Build()
    {
        return key => _creator(_collector[key], Array.Empty<object?>());
    }

    public Func<TKey, T, TProduct> Build<T>()
    {
        return (key, x) => _creator(_collector[key], new object?[] { x });
    }

    public Func<TKey, T1, T2, TProduct> Build<T1, T2>()
    {
        return (key, x1, x2) => _creator(_collector[key], new object?[] { x1, x2 });
    }

    public Func<TKey, T1, T2, T3, TProduct> Build<T1, T2, T3>()
    {
        return (key, x1, x2, x3) => _creator(_collector[key], new object?[] { x1, x2, x3 });
    }

    public Func<TKey, T1, T2, T3, T4, TProduct> Build<T1, T2, T3, T4>()
    {
        return (key, x1, x2, x3, x4) => _creator(_collector[key], new object?[] { x1, x2, x3, x4 });
    }
}

sealed file class CacheBuilder<TKey, TProduct> : ISimpleFactoryBuilder<TKey, TProduct>
{
    private readonly ConcurrentDictionary<ICacheKey<TKey>, TProduct> _cache = new();
    private readonly ISimpleFactoryBuilder<TKey, TProduct> _builder;

    public CacheBuilder(ISimpleFactoryBuilder<TKey, TProduct> builder) => _builder = builder;

    public ISimpleFactoryBuilder<TKey, TProduct> UseCreator(Func<Type, object?[], TProduct> creator)
    {
        return _builder.UseCreator(creator);
    }

    public ISimpleFactoryBuilder<TKey, TProduct> UseCache() => this;

    public Func<TKey, TProduct> Build()
    {
        var build = _builder.Build();
        return key => _cache.GetOrAdd(new CacheKey<TKey>(key), Create);
        TProduct Create(ICacheKey<TKey> cacheKey) => build(cacheKey.Key);
    }

    public Func<TKey, T, TProduct> Build<T>()
    {
        var build = _builder.Build<T>();
        return (key, arg) => _cache.GetOrAdd(new CacheKey<TKey, T>(key, arg), Create);
        TProduct Create(ICacheKey<TKey> cacheKey)
        {
            var args = (CacheKey<TKey, T>)cacheKey;
            return build(args.Key, args.Arg);
        }
    }

    public Func<TKey, T1, T2, TProduct> Build<T1, T2>()
    {
        var build = _builder.Build<T1, T2>();
        return (key, arg1, arg2) => _cache.GetOrAdd(new CacheKey<TKey, T1, T2>(key, arg1, arg2), Create);
        TProduct Create(ICacheKey<TKey> cacheKey)
        {
            var args = (CacheKey<TKey, T1, T2>)cacheKey;
            return build(args.Key, args.Arg1, args.Arg2);
        }
    }

    public Func<TKey, T1, T2, T3, TProduct> Build<T1, T2, T3>()
    {
        var build = _builder.Build<T1, T2, T3>();
        return (key, arg1, arg2, arg3) => _cache.GetOrAdd(new CacheKey<TKey, T1, T2, T3>(key, arg1, arg2, arg3), Create);
        TProduct Create(ICacheKey<TKey> cacheKey)
        {
            var args = (CacheKey<TKey, T1, T2, T3>)cacheKey;
            return build(args.Key, args.Arg1, args.Arg2, args.Arg3);
        }
    }

    public Func<TKey, T1, T2, T3, T4, TProduct> Build<T1, T2, T3, T4>()
    {
        var build = _builder.Build<T1, T2, T3, T4>();
        return (key, arg1, arg2, arg3, arg4) => _cache.GetOrAdd(new CacheKey<TKey, T1, T2, T3, T4>(key, arg1, arg2, arg3, arg4), Create);
        TProduct Create(ICacheKey<TKey> cacheKey)
        {
            var args = (CacheKey<TKey, T1, T2, T3, T4>)cacheKey;
            return build(args.Key, args.Arg1, args.Arg2, args.Arg3, args.Arg4);
        }
    }
}

file interface ICacheKey<out TKey>
{
    TKey Key { get; }
}

readonly file struct CacheKey<TKey> : ICacheKey<TKey>
{
    public TKey Key { get; }

    public CacheKey(TKey key) => Key = key;
}

readonly file struct CacheKey<TKey, T> : ICacheKey<TKey>
{
    public TKey Key { get; }

    public T Arg { get; }

    public CacheKey(TKey key, T arg)
    {
        Key = key;
        Arg = arg;
    }
}

readonly file struct CacheKey<TKey, T1, T2> : ICacheKey<TKey>
{
    public TKey Key { get; }

    public T1 Arg1 { get; }

    public T2 Arg2 { get; }

    public CacheKey(TKey key, T1 arg1, T2 arg2)
    {
        Key = key;
        Arg1 = arg1;
        Arg2 = arg2;
    }
}

readonly file struct CacheKey<TKey, T1, T2, T3> : ICacheKey<TKey>
{
    public TKey Key { get; }

    public T1 Arg1 { get; }

    public T2 Arg2 { get; }

    public T3 Arg3 { get; }

    public CacheKey(TKey key, T1 arg1, T2 arg2, T3 arg3)
    {
        Key = key;
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
    }
}

readonly file struct CacheKey<TKey, T1, T2, T3, T4> : ICacheKey<TKey>
{
    public TKey Key { get; }

    public T1 Arg1 { get; }

    public T2 Arg2 { get; }

    public T3 Arg3 { get; }

    public T4 Arg4 { get; }

    public CacheKey(TKey key, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        Key = key;
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
        Arg4 = arg4;
    }
}
