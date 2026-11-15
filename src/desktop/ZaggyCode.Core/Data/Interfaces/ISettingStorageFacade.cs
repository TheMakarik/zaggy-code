namespace ZaggyCode.Core.Data.Interfaces;

public interface ISettingStorageFacade
{
    public void FlashAllAtBackground();
    public void RevertAllAtBackground();
    public void ToDefaultAllAtBackground();
}