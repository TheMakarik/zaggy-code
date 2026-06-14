namespace ZaggyCode.Data.Interfaces;

public interface IStorage
{
    public ValueTask FlushUpdatesAsync();
    public Task LoadAsync();
 
}