namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingStorageFacade
{
    public Task FlashAllAtBackground();
    public Task RevertAllAtBackground();
    public Task ToDefaultAllAtBackground();
}
