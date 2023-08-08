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
}

public static class SimpleFactory<TKey, TProduct>
{
    private static readonly ConcurrentDictionary<TKey, Type> Storage = new();

    public static IReadOnlyDictionary<TKey, Type> Products => Storage;

    public static void Register(TKey key, Type type) => Storage.TryAdd(key, type);
}
