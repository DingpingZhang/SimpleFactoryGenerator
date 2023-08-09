using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public static class Extensions
{
    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, params object?[] args)
    {
        foreach (var product in SimpleFactory<TKey, TProduct>.Products)
        {
            yield return factory.Create(product.Key, args);
        }
    }

    public static bool Contains<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key)
    {
        return SimpleFactory<TKey, TProduct>.Products.ContainsKey(key);
    }

    public static bool TryCreate<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory, TKey key, out TProduct product, params object?[] args)
    {
        if (factory.Contains(key))
        {
            product = factory.Create(key, args);
            return true;
        }

        product = default!;
        return false;
    }
}
