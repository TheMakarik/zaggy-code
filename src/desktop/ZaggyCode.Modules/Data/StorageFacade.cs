namespace ZaggyCode.Modules.Data;

public sealed class StorageFacade(
    IObservableStorage<UserData> userStorage,
    IGameCodeStorage gameCodeStorage,
    IObservableStorage<PythonSettings> pythonSettingsStorage) : IStorageFacade
{
    public async Task LoadAllAsync()
    {
        await userStorage.LoadAsync();
        await gameCodeStorage.LoadAsync();
        await pythonSettingsStorage.LoadAsync();
    }

    public async ValueTask FlushAllAsync()
    {
        await userStorage.FlushUpdatesAsync();
        await gameCodeStorage.FlushUpdatesAsync();
        await pythonSettingsStorage.FlushUpdatesAsync();
    }
}
