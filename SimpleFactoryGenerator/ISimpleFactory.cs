using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public interface ISimpleFactory<TKey, out TProduct> where TProduct : class
{
    IReadOnlyCollection<TKey> Keys { get; }

    TProduct Create(TKey feed);
}
