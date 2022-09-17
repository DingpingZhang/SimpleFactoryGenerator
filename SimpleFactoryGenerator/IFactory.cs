using System.Collections.Generic;

namespace SimpleFactoryGenerator;

/// <summary>
/// Represents a factory (factory-method pattern).
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
public interface IFactory<in TKey, out TProduct> where TProduct : class
{
    /// <summary>
    /// Gets all the <see cref="ICreator{TKey, TProduct}"/> instances in this factory.
    /// </summary>
    IReadOnlyCollection<ICreator<TKey, TProduct>> Creators { get; }
}
