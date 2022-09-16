namespace SimpleFactoryGenerator;

/// <summary>
/// Represents an entry point to get a factory (factory-method pattern).
/// </summary>
public static class Factory
{
    /// <summary>
    /// Gets a factory by the specified <typeparamref name="TKey"/> and <typeparamref name="TProduct"/> type,
    /// whose instances will be cached.
    /// </summary>
    /// <typeparam name="TKey">The type of the feed used to produce products.</typeparam>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <returns>Returns a factory used to create the <typeparamref name="TProduct"/> instance by the specified <typeparamref name="TKey"/>.</returns>
    public static IFactory<TKey, TProduct> For<TKey, TProduct>() where TProduct : class
    {
        return (IFactory<TKey, TProduct>)GeneratedType.Factory<TProduct>(GetTypeName);

        static string GetTypeName(string @namespace, string targetName)
        {
            return $"SimpleFactoryGenerator.Implementation.GeneratedFactory+{@namespace}_{targetName}";
        }
    }
}
