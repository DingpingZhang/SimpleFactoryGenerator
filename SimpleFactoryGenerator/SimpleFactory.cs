using System;
using System.Collections.Concurrent;

namespace SimpleFactoryGenerator;

public static class SimpleFactory
{
    private static readonly ConcurrentDictionary<string, object> Cache = new();

    public static ISimpleFactory<TKey, TProduct> For<TKey, TProduct>() where TProduct : class
    {
        Type targetType = typeof(TProduct);
        string @namespace = targetType.Namespace.Replace(".", "_");
        string targetName = targetType.Name;
        string typeName = $"SimpleFactoryGenerator.Implementation.Generated+{@namespace}_{targetName}Factory";
        string fullTypeName = $"{typeName}, {targetType.Assembly.FullName}";

        object factory = Cache.GetOrAdd(fullTypeName, CreateFactory);
        return (ISimpleFactory<TKey, TProduct>)factory;
    }

    private static object CreateFactory(string fullTypeName)
    {
        Type type = Type.GetType(fullTypeName);
        return Activator.CreateInstance(type);
    }
}
