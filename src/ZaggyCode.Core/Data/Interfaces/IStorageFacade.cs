namespace ZaggyCode.Core.Data.Interfaces;

public interface IStorageFacade
{
    public Task LoadAllAsync();
    public ValueTask FlushAllAsync();
}