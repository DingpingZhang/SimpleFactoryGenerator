namespace SimpleFactoryGenerator;

public interface ISimpleFactory<in TKey, out TProduct>
{
    TProduct Create(TKey key);
}
