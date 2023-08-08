namespace SimpleFactoryGenerator;

internal interface ICacheKey<out TKey>
{
    TKey Key { get; }
}
