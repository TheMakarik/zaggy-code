using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Model;

namespace ZaggyCode.Modules.BackgroundManagement.Extensions;

public static class BackgroundTaskDtoExtensions
{
    public static BackgroundTask StartAsBackgroundTask(this BackgroundTaskStartDto dto)
    {
        var progress = new Progress<BackgroundProgressMove>();
        var task = Task.Factory.StartNew(() => dto.DelegateToStart(progress), TaskCreationOptions.LongRunning);
        return new BackgroundTask
        {
            Kind = dto.Kind,
            Task = task,
            Progress = progress
        };
    }
}
