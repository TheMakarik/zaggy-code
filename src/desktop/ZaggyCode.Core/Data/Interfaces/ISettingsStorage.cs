namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingsStorage : IStorage
{
    public bool HasChanges { get; }
    public void FlashAtBackground(bool waitBeforeFlash = false);
    public void Revert();
    public ValueTask ToDefaultAsync();
}

public interface ISettingsStorage<out T> : ISettingsStorage
    where T : class, INotifyPropertyChanged
{
    public T Value { get; }
}
