namespace SimpleFactoryGenerator;

/// <summary>
/// Represents a creator of the factory product.
/// </summary>
/// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
/// <typeparam name="TProduct">The type of the product.</typeparam>
public interface ICreator<in TKey, out TProduct> where TProduct : class
{
    /// <summary>
    /// Determines if the specified key can be used to create the product.
    /// </summary>
    /// <param name="key">The specified key.</param>
    /// <returns>Returns a <see cref="bool"/> value to indicate if the specified key can create a product.</returns>
    bool CanCreate(TKey key);

    /// <summary>
    /// Creates a <see cref="TProduct"/> instance by the specified key.
    /// </summary>
    /// <param name="key">The specified key.</param>
    /// <returns>Returns a <see cref="TProduct"/> instance.</returns>
    TProduct Create(TKey key);
}
