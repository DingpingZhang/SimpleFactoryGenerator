using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public interface ISimpleFactory<out TTarget, TKey> where TTarget : class
{
    IReadOnlyCollection<TKey> Keys { get; }

    TTarget Create(TKey key);
}
