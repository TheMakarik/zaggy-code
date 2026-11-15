namespace ZaggyCode.Core.BackgroundManagement;

public interface IBackgroundTaskHelper
{
    public IReadOnlyObservableDictionary<BackgroundTaskKind, BackgroundTask> BackgroundTasks { get; }
    public BackgroundTask StartBackground(BackgroundTaskStartDto startDto);
}