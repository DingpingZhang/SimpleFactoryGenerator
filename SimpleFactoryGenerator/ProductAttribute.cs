using System;

namespace SimpleFactoryGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ProductAttribute<TTarget, TKey> : Attribute where TTarget : class
{
    public ProductAttribute(TKey key)
    {
    }
}
