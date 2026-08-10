namespace ZaggyCode.Core.Data.Interfaces;

public interface IUpdateStorageWaiter
{
    void BeginObserve(INotifyPropertyChanged observable, Func<ValueTask> flushAsync, TimeSpan interval);
}
