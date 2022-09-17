using System;
using System.Collections.Concurrent;

namespace SimpleFactoryGenerator;

internal static class GeneratedType
{
    private static readonly ConcurrentDictionary<string, object> Cache = new();

    public static object Factory<TProduct>(Func<string, string, string> getTypeName) where TProduct : class
    {
        Type targetType = typeof(TProduct);
        string @namespace = targetType.Namespace.Replace(".", "_");
        string targetName = targetType.Name;
        string typeName = getTypeName(@namespace, targetName);
        string fullTypeName = $"{typeName}, {targetType.Assembly.FullName}";

        return Cache.GetOrAdd(fullTypeName, CreateFactory);
    }

    private static object CreateFactory(string fullTypeName)
    {
        Type type = Type.GetType(fullTypeName);
        return Activator.CreateInstance(type);
    }
}
