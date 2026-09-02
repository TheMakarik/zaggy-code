namespace ZaggyCode.Modules.Data;

public sealed class StorageFacade(
    IObservableStorage<UserData> userStorage,
    IGameCodeStorage gameCodeStorage,
    IObservableStorage<PythonSettings> pythonSettingsStorage,
    IObservableStorage<CSharpSettings> csharpSettingsStorage) : IStorageFacade
{
    public async Task LoadAllAsync()
    {
        await userStorage.LoadAsync();
        await gameCodeStorage.LoadAsync();
        await pythonSettingsStorage.LoadAsync();
        await csharpSettingsStorage.LoadAsync();
    }

    public async ValueTask FlushAllAsync()
    {
        await userStorage.FlushUpdatesAsync();
        await gameCodeStorage.FlushUpdatesAsync();
        await pythonSettingsStorage.FlushUpdatesAsync();
        await csharpSettingsStorage.FlushUpdatesAsync();
    }
}
