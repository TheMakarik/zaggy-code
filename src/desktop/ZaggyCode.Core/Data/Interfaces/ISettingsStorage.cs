namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingsStorage<T> : IStorage
{
    public T Value { get; }
    public void Revert();
    public void ToDefault();
}