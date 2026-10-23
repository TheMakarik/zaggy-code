using ObservableCollections;
using ZaggyCode.Core.BackgroundManagement.Model;
using ZaggyCode.Core.BackgroundManagment.Dto;

namespace ZaggyCode.Core.BackgroundManagement;

public interface IBackgroundTaskHelper
{
    public ObservableDictionary<int, BackgroundTask> BackgroundTasks { get; set; }
    public void StartBackground(BackgroundTaskStartDto startDto);
}