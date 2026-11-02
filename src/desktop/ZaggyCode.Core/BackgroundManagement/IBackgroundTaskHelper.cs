using ObservableCollections;
using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Model;

namespace ZaggyCode.Core.BackgroundManagement;

public interface IBackgroundTaskHelper
{
    public ObservableDictionary<int, BackgroundTask> BackgroundTasks { get; set; }
    public void StartBackground(BackgroundTaskStartDto startDto);
}