namespace SimpleFactoryGenerator;

public static class SimpleFactory
{
    public static ISimpleFactory<TKey, TProduct> For<TKey, TProduct>() where TProduct : class
    {
        return (ISimpleFactory<TKey, TProduct>)GeneratedType.Factory<TProduct>(GetTypeName);

        static string GetTypeName(string @namespace, string targetName)
        {
            return $"SimpleFactoryGenerator.Implementation.GeneratedSimpleFactory+{@namespace}_{targetName}";
        }
    }
}
