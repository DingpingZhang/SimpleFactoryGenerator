using System;

namespace SimpleFactoryGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ProductAttribute<TKey, TProduct> : Attribute where TProduct : class
{
    public ProductAttribute(TKey key)
    {
    }
}
