namespace ZaggyCode.Modules.Data;

public sealed class StorageFacade(
    IObservableStorage<StateSettings> stateSettingsStorage,
    IObservableStorage<ViewSettings> viewSettingsStorage,
    IObservableStorage<ThemeSettings> themeSettingsStorage,
    IGameCodeStorage gameCodeStorage,
    IObservableStorage<PythonSettings> pythonSettingsStorage,
    IObservableStorage<CSharpSettings> csharpSettingsStorage) : IStorageFacade
{
    public async Task LoadAllAsync()
    {
        await stateSettingsStorage.LoadAsync();
        await viewSettingsStorage.LoadAsync();
        await themeSettingsStorage.LoadAsync();
        await gameCodeStorage.LoadAsync();
        await pythonSettingsStorage.LoadAsync();
        await csharpSettingsStorage.LoadAsync();
    }

    public async ValueTask FlushAllAsync()
    {
        await stateSettingsStorage.FlushAsync();
        await viewSettingsStorage.FlushAsync();
        await themeSettingsStorage.FlushAsync();
        await gameCodeStorage.FlushAsync();
        await pythonSettingsStorage.FlushAsync();
        await csharpSettingsStorage.FlushAsync();
    }
}
