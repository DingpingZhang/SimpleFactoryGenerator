namespace SimpleFactoryGenerator;

public interface ITags
{
    int Count { get; }

    bool Contains(string name);

    T GetValue<T>(string name);
}
