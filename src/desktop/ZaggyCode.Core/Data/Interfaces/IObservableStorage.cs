namespace ZaggyCode.Core.Data.Interfaces;

//#:NO_AI
public interface IObservableStorage<out T> : IStorage
{
    public T Current { get; }
    public void BeginObserve();
}
