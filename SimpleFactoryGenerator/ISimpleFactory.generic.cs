﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a T4 template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SimpleFactoryGenerator;

public interface ISimpleFactory<in TKey, in T1, out TProduct>
{
    TProduct Create(TKey key, T1 x1);
}

public interface ISimpleFactory<in TKey, in T1, in T2, out TProduct>
{
    TProduct Create(TKey key, T1 x1, T2 x2);
}

public interface ISimpleFactory<in TKey, in T1, in T2, in T3, out TProduct>
{
    TProduct Create(TKey key, T1 x1, T2 x2, T3 x3);
}

public interface ISimpleFactory<in TKey, in T1, in T2, in T3, in T4, out TProduct>
{
    TProduct Create(TKey key, T1 x1, T2 x2, T3 x3, T4 x4);
}

public interface ISimpleFactory<in TKey, in T1, in T2, in T3, in T4, in T5, out TProduct>
{
    TProduct Create(TKey key, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5);
}

public interface ISimpleFactory<in TKey, in T1, in T2, in T3, in T4, in T5, in T6, out TProduct>
{
    TProduct Create(TKey key, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6);
}

public interface ISimpleFactory<in TKey, in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TProduct>
{
    TProduct Create(TKey key, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7);
}

public interface ISimpleFactory<in TKey, in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TProduct>
{
    TProduct Create(TKey key, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7, T8 x8);
}

