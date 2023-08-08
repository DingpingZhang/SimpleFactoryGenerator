using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public static partial class Extensions
{
    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(this ISimpleFactory<TKey, TProduct> factory)
    {
        foreach (var product in SimpleFactory<TKey, TProduct>.Products)
        {
            yield return factory.Create(product.Key);
        }
    }
}
