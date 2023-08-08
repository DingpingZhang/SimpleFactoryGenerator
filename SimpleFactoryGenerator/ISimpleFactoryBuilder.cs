using System;

namespace SimpleFactoryGenerator;

public interface ISimpleFactoryBuilder<in TKey, TProduct>
{
    ISimpleFactoryBuilder<TKey, TProduct> UseCreator(Func<Type, object?[], TProduct> creator);

    ISimpleFactoryBuilder<TKey, TProduct> UseCache();

    Func<TKey, TProduct> Build();

    Func<TKey, T, TProduct> Build<T>();

    Func<TKey, T1, T2, TProduct> Build<T1, T2>();

    Func<TKey, T1, T2, T3, TProduct> Build<T1, T2, T3>();

    Func<TKey, T1, T2, T3, T4, TProduct> Build<T1, T2, T3, T4>();
}
