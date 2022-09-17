using System.Collections.Generic;

namespace SimpleFactoryGenerator;

/// <summary>
/// Represents a simple-factory.
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
public interface ISimpleFactory<TKey, out TProduct> where TProduct : class
{
    /// <summary>
    /// Gets all the <see cref="TKey"/> instances in this factory.
    /// </summary>
    IReadOnlyCollection<TKey> Keys { get; }

    /// <summary>
    /// Creates a product instance by the specified key.
    /// </summary>
    /// <param name="key">The specified key.</param>
    /// <returns>Returns a <see cref="TProduct"/> instance.</returns>
    TProduct Create(TKey key);
}
