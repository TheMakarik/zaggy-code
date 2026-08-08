namespace ZaggyCode.Modules.Data;

public sealed class StorageFacade(
    IUserStorage userStorage,
    IGameCodeStorage gameCodeStorage,
    IPythonSettingsStorage pythonSettingsStorage) : IStorageFacade
{
    public Task LoadAllAsync()
    {
        return userStorage.LoadAsync()
            .ContinueWith((_) => gameCodeStorage.LoadAsync())
            .ContinueWith((_) => pythonSettingsStorage.LoadAsync());
    }

    public async ValueTask FlushAllAsync()
    {
        await userStorage.FlushUpdatesAsync();
        await gameCodeStorage.FlushUpdatesAsync();
        await pythonSettingsStorage.FlushUpdatesAsync();
    }
}
