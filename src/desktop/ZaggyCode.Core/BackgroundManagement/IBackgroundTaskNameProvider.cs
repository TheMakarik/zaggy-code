namespace ZaggyCode.Core.BackgroundManagement;

public interface IBackgroundTaskNameProvider
{
    public string GetName(BackgroundTaskKind kind);
}
