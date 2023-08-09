using System;

namespace SimpleFactoryGenerator;

public interface ISimpleFactory<in TKey, TProduct>
{
    ISimpleFactory<TKey, TProduct> WithCreator(Func<Type, object?[], TProduct> creator);

    ISimpleFactory<TKey, TProduct> WithCache();

    TProduct Create(TKey key, params object?[] args);
}
