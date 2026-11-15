namespace ZaggyCode.Core.Data.Interfaces;

public interface IStorage
{
    public ValueTask FlushAsync();
    public Task LoadAsync();
}
