using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public class SimpleFactory
{
    public static ISimpleFactory<TKey, TProduct> For<TKey, TProduct>()
    {
        return new SimpleFactory<TKey, TProduct>(SimpleFactory<TKey, TProduct>.Products);
    }
}

public class SimpleFactory<TKey, TProduct> : ISimpleFactory<TKey, TProduct>
{
    private static readonly ConcurrentDictionary<TKey, Type> Storage = new();

    public static IReadOnlyDictionary<TKey, Type> Products => Storage;

    public static void Register(TKey key, Type type) => Storage.TryAdd(key, type);

    private readonly IReadOnlyDictionary<TKey, Type> _collector;

    private Func<Type, object?[], TProduct> _creator = (type, args) => (TProduct)Activator.CreateInstance(type, args);

    public SimpleFactory(IReadOnlyDictionary<TKey, Type> collector) => _collector = collector;

    public ISimpleFactory<TKey, TProduct> WithCreator(Func<Type, object?[], TProduct> creator)
    {
        _creator = creator;
        return this;
    }

    public ISimpleFactory<TKey, TProduct> WithCache()
    {
        return new CacheSimpleFactory<TKey, TProduct>(this);
    }

    public TProduct Create(TKey key, params object?[] args) => _creator(_collector[key], args);
}
