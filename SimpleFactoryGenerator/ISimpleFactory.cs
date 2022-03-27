namespace SimpleFactoryGenerator
{
    public interface ISimpleFactory<out TTarget, in TKey>
    {
        TTarget Create(TKey key);
    }
}
