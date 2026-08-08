namespace ZaggyCode.Core.Utils;

public interface IUpdateStorageWaiter
{
    void BeginObserve(INotifyPropertyChanged observable, Func<ValueTask> flushAsync, TimeSpan interval);
}
