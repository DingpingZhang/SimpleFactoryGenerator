using System.Collections.Generic;

namespace SimpleFactoryGenerator;

public interface IFactory<in TKey, out TProduct> where TProduct : class
{
    IReadOnlyCollection<ICreator<TKey, TProduct>> Creators { get; }
}
