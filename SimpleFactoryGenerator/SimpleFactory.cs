using System;
using System.Collections.Concurrent;

namespace SimpleFactoryGenerator
{
    public static class SimpleFactory
    {
        private static readonly ConcurrentDictionary<string, object> Cache = new();

        public static ISimpleFactory<TTarget, TKey> For<TTarget, TKey>()
        {
            Type targetType = typeof(TTarget);
            string @namespace = targetType.Namespace.Replace(".", "_");
            string targetName = targetType.Name;
            string typeName = $"SimpleFactoryGenerator.Implementation.Generated+{@namespace}_{targetName}Factory";
            string fullTypeName = $"{typeName}, {targetType.Assembly.FullName}";

            object factory = Cache.GetOrAdd(fullTypeName, CreateFactory);
            return (ISimpleFactory<TTarget, TKey>)factory;
        }

        private static object CreateFactory(string fullTypeName)
        {
            Type type = Type.GetType(fullTypeName);
            return Activator.CreateInstance(type);
        }
    }
}
