namespace ZaggyCode.Modules.Data;

public sealed class PythonSettingsStorage(
    ILogger<PythonSettingsStorage> logger,
    IOptions<StorageOptions> storageOptions,
    IOptions<PythonDefaultSettingsOptions> defaultSettings,
    IUpdateStorageWaiter updateWaiter,
    ISpecialFolderProvider folderProvider) : IPythonSettingsStorage
{
    public PythonSettings Current { get => field ?? throw new InvalidOperationException("Cannot load null python settings."); private set; }

    public void BeginObserve()
    {
        logger.LogInformation("Observe python settings {path}", storageOptions.Value.PythonSettingsPath);
        updateWaiter.BeginObserve(
            Current,
            FlushUpdatesAsync,
            TimeSpan.FromSeconds(storageOptions.Value.WaitUserDataUpdateSeconds));
    }

    public ValueTask FlushUpdatesAsync()
    {
        return SaveAsync();
    }

    public async Task LoadAsync()
    {
        var filePath = folderProvider.GetFolder(Environment.SpecialFolder.ApplicationData, storageOptions.Value.PythonSettingsPath);
        logger.LogInformation("Begin loading python settings from path {path}", filePath);

        if (!File.Exists(filePath))
        {
            await CreateDefaultSettingsFileAsync(filePath);
            BeginObserve();
            return;
        }

        try
        {
            await using var file = File.Open(filePath, FileMode.Open);
            Current = await JsonSerializer.DeserializeAsync(file, Json.PythonSettingsSerializerContext.Default.PythonSettings)
                      ?? throw new InvalidOperationException("Python settings file corrupted");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading python settings from path {path}", filePath);
            File.Delete(filePath);
            await CreateDefaultSettingsFileAsync(filePath);
        }

        BeginObserve();
    }

    private async Task CreateDefaultSettingsFileAsync(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        logger.LogInformation("Python settings were not found. Creating default settings file {path}", filePath);
        await using var file = File.Create(filePath);
        Current = defaultSettings.Value.Settings;
        await JsonSerializer.SerializeAsync(file, Current, Json.PythonSettingsSerializerContext.Default.PythonSettings);
    }

    private async ValueTask SaveAsync()
    {
        var filePath = folderProvider.GetFolder(Environment.SpecialFolder.ApplicationData, storageOptions.Value.PythonSettingsPath);
        await using (var file = File.Open(filePath, FileMode.Truncate))
            await JsonSerializer.SerializeAsync(file, Current, Json.PythonSettingsSerializerContext.Default.PythonSettings);

        logger.LogInformation("Python settings file saved successfully. Content: {content}", await File.ReadAllTextAsync(filePath));
    }
}
