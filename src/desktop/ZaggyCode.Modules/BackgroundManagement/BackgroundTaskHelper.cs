namespace ZaggyCode.Modules.BackgroundManagement;

public sealed class BackgroundTaskHelper(ILogger<IBackgroundTaskHelper> logger) : IBackgroundTaskHelper
{
    public ObservableDictionary<int, BackgroundTask> BackgroundTasks { get; set; } = [];
    
    public void StartBackground(BackgroundTaskStartDto startDto)
    {
        var backgroundTask = startDto.StartAsBackgroundTask();
        BackgroundTasks.Add(backgroundTask.Task.Id, backgroundTask);
        
        backgroundTask.Task.ContinueWith(task =>
        {
            logger.LogDebug($"Заканчивается выполнение бэкграунд задачи с ID: {task.Id} и названием \"{backgroundTask.Name}\"");
            BackgroundTasks.Remove(task.Id);
        });
    }
}