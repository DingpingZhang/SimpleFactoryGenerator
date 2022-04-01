using System;

namespace SimpleFactoryGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ProductOfSimpleFactoryAttribute<TTarget, TKey> : Attribute where TTarget : class
    {
        public ProductOfSimpleFactoryAttribute(TKey key)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ProductOfSimpleFactoryAttribute : Attribute
    {
        public ProductOfSimpleFactoryAttribute(Type targetType, Type keyType, object key)
        {
        }
    }
}
