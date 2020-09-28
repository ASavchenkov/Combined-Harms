using Godot;

public interface IProvider
{
    Node GenerateObserver(string name);
}

public interface IObserver<T> where T: IProvider
{
    void Init(T provider);
}