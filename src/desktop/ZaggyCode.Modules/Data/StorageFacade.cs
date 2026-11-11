namespace ZaggyCode.Modules.Data;

public sealed class StorageFacade(
    IObservableStorage<StateSettings> stateSettingsStorage,
    IObservableStorage<ViewSettings> viewSettingsStorage,
    IObservableStorage<ThemeSettings> themeSettingsStorage,
    IGameCodeStorage gameCodeStorage,
    IObservableStorage<PythonSettings> pythonSettingsStorage,
    IObservableStorage<CSharpSettings> csharpSettingsStorage,
    IOptions<DefaultViewSettings> defaultViewSettings) : IStorageFacade
{
    public async Task LoadAllAsync()
    {
        await stateSettingsStorage.LoadAsync();
        await viewSettingsStorage.LoadAsync();
        MigrateViewSettings(viewSettingsStorage.Current);
        await themeSettingsStorage.LoadAsync();
        await gameCodeStorage.LoadAsync();
        await pythonSettingsStorage.LoadAsync();
        await csharpSettingsStorage.LoadAsync();
    }

    public async ValueTask FlushAllAsync()
    {
        await stateSettingsStorage.FlushUpdatesAsync();
        await viewSettingsStorage.FlushUpdatesAsync();
        await themeSettingsStorage.FlushUpdatesAsync();
        await gameCodeStorage.FlushUpdatesAsync();
        await pythonSettingsStorage.FlushUpdatesAsync();
        await csharpSettingsStorage.FlushUpdatesAsync();
    }

    // В settings.json старых версий поля DescriptionFontSize ещё не было —
    // JsonSerializer оставляет для него 0, поэтому подставляем значение по умолчанию.
    private void MigrateViewSettings(ViewSettings settings)
    {
        if (settings.DescriptionFontSize <= 0)
            settings.DescriptionFontSize = defaultViewSettings.Value.Settings.DescriptionFontSize;
    }
}
