namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingStorageFacade
{
    public Task LoadAllAsync();
    public Task FlushAllAsync();
    public void FlashAllAtBackground();
    public void RevertAllAtBackground();
    public void ToDefaultAllAtBackground();
}
