namespace ZaggyCode.Modules.Data;

//#:NO_AI
public sealed class StorageFacade(
    ISettingsStorage<StateSettings> stateSettingsStorage,
    ISettingsStorage<ViewSettings> viewSettingsStorage,
    ISettingsStorage<ThemeSettings> themeSettingsStorage,
    ISettingsStorage<PythonSettings> pythonSettingsStorage,
    ISettingsStorage<CSharpSettings> csharpSettingsStorage,
    IGameCodeStorage gameCodeStorage,
    ILogger<StorageFacade> logger) : IStorageFacade
{
    public async Task LoadAllAsync()
    {
        logger.LogInformation("Загрузка всех хранилищ: настройки");
        
        await Task.WhenAll(SettingsStorages.Select(storage => storage.LoadAsync()));

        logger.LogInformation("Загрузка кода игр из {path}", gameCodeStorage.GetType().Name);
        await gameCodeStorage.LoadAsync();

        logger.LogInformation("Все хранилища загружены");
    }

    public async Task FlushAsync()
    {
        logger.LogInformation("Сохранение всех хранилищ: настройки");

        foreach (var storage in SettingsStorages)
        {
            logger.LogDebug("Сохранение {storage}", storage.GetType().Name);
            await storage.FlushAsync();
        }

        logger.LogInformation("Сохранение кода игр");
        await gameCodeStorage.FlushAsync();

        logger.LogInformation("Все хранилища сохранены");
    }

    private IReadOnlyList<ISettingsStorage> SettingsStorages =>
    [
        stateSettingsStorage,
        viewSettingsStorage,
        themeSettingsStorage,
        pythonSettingsStorage,
        csharpSettingsStorage
    ];
}
