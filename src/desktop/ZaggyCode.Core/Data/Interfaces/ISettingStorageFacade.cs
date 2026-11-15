namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingStorageFacade
{
    public Task LoadAllAsync();
    public void FlashAllAtBackground();
    public void RevertAllAtBackground();
    public void ToDefaultAllAtBackground();
}
