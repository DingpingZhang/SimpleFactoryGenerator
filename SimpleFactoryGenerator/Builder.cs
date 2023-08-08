using System;
using System.Collections.Generic;

namespace SimpleFactoryGenerator;

internal partial class Builder<TKey, TProduct> : ISimpleFactoryBuilder<TKey, TProduct>
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

    public ISimpleFactory<TKey, TProduct> Build()
    {
        return new SimpleFactoryImpl<TKey, TProduct>(key => _creator(_collector[key], Array.Empty<object?>()));
    }
}
