namespace ZaggyCode.Core.Data.Interfaces;

public interface IStorage
{
    public ValueTask FlushUpdatesAsync();
    public Task LoadAsync();
 
}