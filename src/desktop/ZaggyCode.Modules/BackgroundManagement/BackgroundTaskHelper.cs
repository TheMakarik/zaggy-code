namespace ZaggyCode.Modules.BackgroundManagement;

//#:NO_AI
public sealed class BackgroundTaskHelper(ILogger<IBackgroundTaskHelper> logger) : IBackgroundTaskHelper
{
    private readonly ObservableDictionary<BackgroundTaskKind, BackgroundTask> _backgroundTasks = [];

    public IReadOnlyObservableDictionary<BackgroundTaskKind, BackgroundTask> BackgroundTasks => _backgroundTasks;

    public BackgroundTask StartBackground(BackgroundTaskStartDto startDto)
    {
        var backgroundTask = startDto.StartAsBackgroundTask();
        var progressSubscription = SubscribeProgress(backgroundTask);

        _backgroundTasks[startDto.Kind] = backgroundTask;

        backgroundTask.Task.ContinueWith(task =>
        {
            progressSubscription?.Dispose();

            logger.LogDebug("Заканчивается выполнение фоновой задачи с типом {kind}", backgroundTask.Kind);

            if (_backgroundTasks.TryGetValue(startDto.Kind, out var currentTask) && currentTask.Task == task)
                _backgroundTasks.Remove(startDto.Kind);
        });

        return backgroundTask;
    }

    private ProgressSubscription? SubscribeProgress(BackgroundTask backgroundTask)
    {
        if (backgroundTask.Progress is not Progress<BackgroundProgressMove> progress)
            return null;

        void Handler(object? sender, BackgroundProgressMove move)
        {
            backgroundTask.Description = move.DescriptionOfNextMove;

            logger.LogDebug("Фоновая задача {kind}: {description}", move.Kind, move.DescriptionOfNextMove);
        }

        progress.ProgressChanged += Handler;
        return new ProgressSubscription(progress, Handler);
    }
}
