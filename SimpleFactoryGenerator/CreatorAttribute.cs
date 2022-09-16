using System;

namespace SimpleFactoryGenerator;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class CreatorAttribute<TKey, TProduct> : Attribute where TProduct : class
{
}
