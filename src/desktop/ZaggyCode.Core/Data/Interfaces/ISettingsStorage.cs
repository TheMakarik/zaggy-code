namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingsStorage<T> : IStorage
{
    public void Revert();
    public void ToDefault();
}