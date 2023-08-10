using System;

namespace SimpleFactoryGenerator;

/// <summary>
/// Represents a simple-factory.
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
public interface ISimpleFactory<in TKey, TProduct>
{
    /// <summary>
    /// Replaces this factory's internal creator.
    /// </summary>
    /// <param name="creator">Creator for creating instances based on type.</param>
    /// <returns>Returns itself.</returns>
    ISimpleFactory<TKey, TProduct> WithCreator(Func<Type, object?[], TProduct> creator);

    /// <summary>
    /// Creates a product instance by the specified key.
    /// </summary>
    /// <param name="key">The specified key.</param>
    /// <param name="args">The arguments required by the constructor that creates this instance.</param>
    /// <returns>Returns a <see cref="TProduct"/> instance.</returns>
    TProduct Create(TKey key, params object?[] args);
}
