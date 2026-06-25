namespace ZaggyCode.Data.Interfaces;

public interface IObservableStorage<out T> : IStorage
{
    public T Current { get; }
    public void BeginObserve();
}