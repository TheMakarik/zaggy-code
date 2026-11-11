namespace ZaggyCode.Modules.BackgroundManagement;

public sealed class BackgroundTaskHelper(ILogger<IBackgroundTaskHelper> logger) : IBackgroundTaskHelper
{
    private readonly ObservableDictionary<BackgroundTaskKind, BackgroundTask> _backgroundTasks = [];

    public IReadOnlyObservableDictionary<BackgroundTaskKind, BackgroundTask> BackgroundTasks => _backgroundTasks;

    public void StartBackground(BackgroundTaskStartDto startDto)
    {
        var backgroundTask = startDto.StartAsBackgroundTask();
        ObserveDescription(backgroundTask);

        _backgroundTasks[startDto.Kind] = backgroundTask;

        backgroundTask.Task.ContinueWith(task =>
        {
            logger.LogDebug($"Заканчивается выполнение фоновой задачи с типом {backgroundTask.Kind}");

            if (_backgroundTasks.TryGetValue(startDto.Kind, out var currentTask) && currentTask.Task == task)
                _backgroundTasks.Remove(startDto.Kind);
        });
    }

    private static void ObserveDescription(BackgroundTask backgroundTask)
    {
        if (backgroundTask.Progress is not Progress<BackgroundProgressMove> progress)
            return;

        progress.ProgressChanged += (_, move) => backgroundTask.Description = move.DescriptionOfNextMove;
    }
}
