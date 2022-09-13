using System.Collections.Generic;
using System.Linq;

namespace SimpleFactoryGenerator;

public static class FactoryExtensions
{
    public static IFactory<TKey, TProduct> WithCache<TKey, TProduct>(this IFactory<TKey, TProduct> factory) where TProduct : class
    {
        return new CachedFactory<TKey, TProduct>(factory);
    }

    public static bool Contains<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.Creators.Any(creator => creator.CanCreate(key));
    }

    public static IEnumerable<TProduct> CreateAll<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.Creators
            .Where(creator => creator.CanCreate(key))
            .Select(creator => creator.Create(key));
    }

    public static TProduct CreateFirst<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key) where TProduct : class
    {
        return factory.CreateAll(key).First();
    }

    public static bool TryCreateFirst<TKey, TProduct>(this IFactory<TKey, TProduct> factory, TKey key, out TProduct product) where TProduct : class
    {
        TProduct? temp = factory.CreateAll(key).FirstOrDefault();
        product = temp!;
        return temp is not null;
    }

    private sealed class CachedFactory<TKey, TProduct> : IFactory<TKey, TProduct> where TProduct : class
    {
        public IReadOnlyCollection<ICreator<TKey, TProduct>> Creators { get; }

        public CachedFactory(IFactory<TKey, TProduct> factory)
        {
            Creators = factory.Creators.Select(creator => new CachedCreator(creator)).ToArray();
        }

        private sealed class CachedCreator : ICreator<TKey, TProduct>
        {
            private readonly ICreator<TKey, TProduct> _creator;

            private TProduct? _product;

            public CachedCreator(ICreator<TKey, TProduct> creator) => _creator = creator;

            public bool CanCreate(TKey key) => _creator.CanCreate(key);

            public TProduct Create(TKey key) => _product ??= _creator.Create(key);
        }
    }
}
