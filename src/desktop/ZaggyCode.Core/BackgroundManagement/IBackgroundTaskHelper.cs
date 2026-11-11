namespace ZaggyCode.Core.BackgroundManagement;

public interface IBackgroundTaskHelper
{
    public IReadOnlyObservableDictionary<BackgroundTaskKind, BackgroundTask> BackgroundTasks { get; }
    public void StartBackground(BackgroundTaskStartDto startDto);
}