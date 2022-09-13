namespace SimpleFactoryGenerator;

public interface ICreator<in TKey, out TProduct> where TProduct : class
{
    bool CanCreate(TKey key);

    TProduct Create(TKey key);
}
