using System;

namespace SimpleFactoryGenerator;

internal class SimpleFactoryImpl<TKey, TProduct> : ISimpleFactory<TKey, TProduct>
{
    private readonly Func<TKey, TProduct> _create;

    public SimpleFactoryImpl(Func<TKey, TProduct> create) => _create = create;

    public TProduct Create(TKey key) => _create(key);
}
