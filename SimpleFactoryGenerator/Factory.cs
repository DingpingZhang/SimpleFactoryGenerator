namespace SimpleFactoryGenerator;

public static class Factory
{
    public static IFactory<TKey, TProduct> For<TKey, TProduct>() where TProduct : class
    {
        return (IFactory<TKey, TProduct>)GeneratedType.Factory<TProduct>(GetTypeName);

        static string GetTypeName(string @namespace, string targetName)
        {
            return $"SimpleFactoryGenerator.Implementation.GeneratedFactory+{@namespace}_{targetName}";
        }
    }
}
