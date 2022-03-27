using System;

namespace SimpleFactoryGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ProductOfSimpleFactoryAttribute<TTarget, TKey> : Attribute where TTarget : class
    {
        public TKey Key { get; }

        public ProductOfSimpleFactoryAttribute(TKey key)
        {
            Key = key;
        }
    }
}
