namespace ZaggyCode.Modules.Data;

//#:NO_AI
public sealed class ObservableStorage<T>(
    ILogger logger,
    string filePath,
    TimeSpan updateInterval,
    JsonTypeInfo<T> jsonTypeInfo,
    IUpdateStorageWaiter updateWaiter,
    Func<T> createDefault) : IObservableStorage<T>
    where T : class, INotifyPropertyChanged
{
    private T? _current;

    public T Current
    {
        get => _current ?? throw new InvalidOperationException($"Cannot load null {typeof(T).Name}.");
        private set => _current = value;
    }

    public void BeginObserve()
    {
        logger.LogInformation("Наблюдение за {type} по пути {path}", typeof(T).Name, filePath);
        updateWaiter.BeginObserve(Current, FlushUpdatesAsync, updateInterval);
    }

    public ValueTask FlushUpdatesAsync()
    {
        return SaveAsync();
    }

    public async Task LoadAsync()
    {
        logger.LogInformation("Начало загрузки {type} из {path}", typeof(T).Name, filePath);

        if (!File.Exists(filePath))
        {
            await CreateDefaultFileAsync();
            BeginObserve();
            return;
        }

        try
        {
            await using var file = File.Open(filePath, FileMode.Open);
            Current = await JsonSerializer.DeserializeAsync(file, jsonTypeInfo)
                      ?? throw new InvalidOperationException($"{typeof(T).Name} file corrupted");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка загрузки {type} из {path}", typeof(T).Name, filePath);
            File.Delete(filePath);
            await CreateDefaultFileAsync();
        }

        BeginObserve();
    }

    private async Task CreateDefaultFileAsync()
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        logger.LogInformation("{type} не найден. Создание файла по умолчанию {path}", typeof(T).Name, filePath);
        await using var file = File.Create(filePath);
        Current = createDefault();
        await JsonSerializer.SerializeAsync(file, Current, jsonTypeInfo);
    }

    private async ValueTask SaveAsync()
    {
        await using (var file = File.Open(filePath, FileMode.Truncate))
            await JsonSerializer.SerializeAsync(file, Current, jsonTypeInfo);

        logger.LogInformation("Файл {type} успешно сохранён. Содержимое: {content}", typeof(T).Name, await File.ReadAllTextAsync(filePath));
    }
}
