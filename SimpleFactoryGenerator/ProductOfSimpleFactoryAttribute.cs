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
}
