using System;

namespace SimpleFactoryGenerator;

public delegate TProduct ProductCreator<in TKey, out TProduct>(Type productType, ITags tags, TKey key, object?[] args);

/// <summary>
/// Represents a simple-factory.
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
public interface ISimpleFactory<TKey, TProduct>
{
    /// <summary>
    /// Replaces this factory's internal creator.
    /// </summary>
    /// <param name="creator">Creator for creating instances based on type.</param>
    /// <returns>Returns itself.</returns>
    ISimpleFactory<TKey, TProduct> WithCreator(ProductCreator<TKey, TProduct> creator);

    /// <summary>
    /// Creates a product instance by the specified key.
    /// </summary>
    /// <param name="key">The specified key.</param>
    /// <param name="args">The arguments required by the constructor that creates this instance.</param>
    /// <returns>Returns a <typeparamref name="TProduct"/> instance.</returns>
    TProduct Create(TKey key, params object?[] args);
}
