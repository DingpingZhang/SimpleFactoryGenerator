using System;

namespace SimpleFactoryGenerator;

public partial interface ISimpleFactoryBuilder<in TKey, TProduct>
{
    ISimpleFactoryBuilder<TKey, TProduct> UseCreator(Func<Type, object?[], TProduct> creator);

    ISimpleFactoryBuilder<TKey, TProduct> UseCache();

    ISimpleFactory<TKey, TProduct> Build();
}
