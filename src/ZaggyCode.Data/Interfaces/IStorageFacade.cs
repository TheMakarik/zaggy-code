using ZaggyCode.Data.Model;

namespace ZaggyCode.Data.Interfaces;

public interface IStorageFacade
{
    public Task LoadAllAsync();
    public ValueTask FlushAllAsync();
}