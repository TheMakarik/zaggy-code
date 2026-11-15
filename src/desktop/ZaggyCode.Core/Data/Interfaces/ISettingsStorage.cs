namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingsStorage<out T> : IStorage
    where T : class, INotifyPropertyChanged
{
    public T Value { get; }
    public bool HasChanges { get; }
    public void FlashAtBackground(bool waitBeforeFlash = false);
    public void Revert();
    public ValueTask ToDefaultAsync();
}
