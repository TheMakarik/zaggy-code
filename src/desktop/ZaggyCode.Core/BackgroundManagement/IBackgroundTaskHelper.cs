using ObservableCollections;
using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Enums;
using ZaggyCode.Core.BackgroundManagement.Model;

namespace ZaggyCode.Core.BackgroundManagement;

public interface IBackgroundTaskHelper
{
    public IReadOnlyObservableDictionary<BackgroundTaskKind, BackgroundTask> BackgroundTasks { get; }
    public void StartBackground(BackgroundTaskStartDto startDto);
}