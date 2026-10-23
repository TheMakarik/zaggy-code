namespace ZaggyCode.Modules.BackgroundManagement.Extensions;

public static class BackgroundTaskDtoExtensions
{
    public static BackgroundTask StartAsBackgroundTask(this BackgroundTaskStartDto dto)
    {
        var progress = new Progress<double>();
        var task = Task.Factory.StartNew(() => dto.DelegateToStart(progress));
        return new BackgroundTask()
        {
            Task = task,
            Description = dto.Description,
            Name = dto.Name,
            Progress = progress
        };
    }
}