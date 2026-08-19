namespace ZaggyCode.Core.Data.Interfaces;

//#:NO_AI
public interface IStorageFacade
{
    public Task LoadAllAsync();
    public ValueTask FlushAllAsync();
}
