using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Model;

namespace ZaggyCode.Modules.BackgroundManagement.Extensions;

public static class BackgroundTaskDtoExtensions
{
    public static BackgroundTask StartAsBackgroundTask(this BackgroundTaskStartDto dto)
    {
        var progress = new Progress<BackgroundProgressMove>();
        var task = Task.Run(() => dto.DelegateToStart(progress));
        return new BackgroundTask
        {
            Kind = dto.Kind,
            Task = task,
            HasNoProgress = dto.HasNoProgress,
            Progress = progress
        };
    }
}
