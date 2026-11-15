using System.Text.Json.Serialization.Metadata;

namespace ZaggyCode.Modules.Data;

//#:NO_AI
public sealed class SettingsStorage<T> : ISettingsStorage<T>
    where T : class, INotifyPropertyChanged
{
    private readonly ILogger logger;
    private readonly string filePath;
    private readonly JsonTypeInfo<T> jsonTypeInfo;
    private readonly Func<T> createDefault;
    private readonly IBackgroundTaskHelper backgroundTaskHelper;
    private readonly BackgroundTaskKind backgroundKind;
    private readonly TimeSpan waitBeforeFlashInterval;
    private readonly Lock stateLock = new();
    private readonly Lock flashLock = new();
    private readonly SemaphoreSlim flushGate = new(1, 1);

    private T? _value;
    private string _persistedJson = string.Empty;
    private CancellationTokenSource? _pendingFlash;

    public SettingsStorage(
        ILogger<SettingsStorage<T>> logger,
        string filePath,
        IBackgroundTaskHelper backgroundTaskHelper,
        BackgroundTaskKind backgroundKind,
        TimeSpan waitBeforeFlashInterval,
        JsonTypeInfo<T> jsonTypeInfo,
        Func<T> createDefault)
    {
        this.logger = logger;
        this.filePath = filePath;
        this.backgroundTaskHelper = backgroundTaskHelper;
        this.backgroundKind = backgroundKind;
        this.waitBeforeFlashInterval = waitBeforeFlashInterval;
        this.jsonTypeInfo = jsonTypeInfo;
        this.createDefault = createDefault;
    }

    public T Value => _value ?? throw new InvalidOperationException($"Не удалось загрузить {typeof(T).Name}: значение отсутствует.");

    public bool HasChanges
    {
        get
        {
            lock (stateLock)
                return !IsCurrentEqualToPersisted();
        }
    }

    public async Task LoadAsync()
    {
        logger.LogInformation("Начало загрузки {type} из {path}", typeof(T).Name, filePath);

        if (!File.Exists(filePath))
        {
            await CreateDefaultFileAsync();
            return;
        }

        try
        {
            await using var file = File.OpenRead(filePath);
            var loaded = await JsonSerializer.DeserializeAsync(file, jsonTypeInfo)
                         ?? throw new InvalidOperationException($"Файл {typeof(T).Name} повреждён");
            StoreLoaded(loaded);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка загрузки {type} из {path}", typeof(T).Name, filePath);
            File.Delete(filePath);
            await CreateDefaultFileAsync();
        }
    }

    public void Revert()
    {
        string baseline;
        lock (stateLock)
            baseline = _persistedJson;

        if (baseline.Length == 0)
            return;

        logger.LogDebug("Отмена изменений {type} до сохранённого состояния", typeof(T).Name);

        var restored = JsonSerializer.Deserialize(baseline, jsonTypeInfo);
        if (restored is null)
            return;

        SettingsCopier.Copy(restored, Value);
        logger.LogInformation("Изменения {type} отменены до сохранённого состояния", typeof(T).Name);
    }

    public async ValueTask FlushAsync()
    {
        await flushGate.WaitAsync();
        try
        {
            string json;
            lock (stateLock)
                json = JsonSerializer.Serialize(Value, jsonTypeInfo);

            await WriteFileAsync(json);

            lock (stateLock)
                _persistedJson = json;

            logger.LogInformation("Файл {type} успешно сохранён по пути {path}", typeof(T).Name, filePath);
        }
        finally
        {
            flushGate.Release();
        }
    }

    public async ValueTask ToDefaultAsync()
    {
        var defaults = Clone(createDefault());
        SettingsCopier.Copy(defaults, Value);
        await FlushAsync();
        logger.LogInformation("{type} сброшены к значениям по умолчанию", typeof(T).Name);
    }

    public Task FlashAtBackground(bool waitBeforeFlash = false)
    {
        logger.LogDebug(
            "Фоновое сохранение {type}: ожидание перед сохранением — {wait}",
            typeof(T).Name,
            waitBeforeFlash);

        var backgroundTask = backgroundTaskHelper.StartBackground(new BackgroundTaskStartDto
        {
            Kind = backgroundKind,
            HasNoProgress = true,
            DelegateToStart = _ => FlashCoreAsync(waitBeforeFlash)
        });

        return backgroundTask.Task;
    }

    private async Task FlashCoreAsync(bool waitBeforeFlash)
    {
        if (!waitBeforeFlash)
        {
            await FlushOrLogAsync();
            return;
        }

        if (await WaitForChangesToSettleAsync())
            await FlushOrLogAsync();
    }

    private async Task<bool> WaitForChangesToSettleAsync()
    {
        CancellationTokenSource cancellation;
        lock (flashLock)
        {
            _pendingFlash?.Cancel();
            _pendingFlash?.Dispose();
            cancellation = new CancellationTokenSource();
            _pendingFlash = cancellation;
        }

        try
        {
            await Task.Delay(waitBeforeFlashInterval, cancellation.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task FlushOrLogAsync()
    {
        try
        {
            await FlushAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось сохранить {type} в {path}", typeof(T).Name, filePath);
        }
    }

    private bool IsCurrentEqualToPersisted()
        => JsonSerializer.Serialize(Value, jsonTypeInfo) == _persistedJson;

    private void StoreLoaded(T loaded)
    {
        var json = JsonSerializer.Serialize(loaded, jsonTypeInfo);
        lock (stateLock)
        {
            _value = loaded;
            _persistedJson = json;
        }
    }

    private async Task CreateDefaultFileAsync()
    {
        var directory = Path.GetDirectoryName(filePath);
        if (ShouldCreateDirectory(directory))
            Directory.CreateDirectory(directory!);

        logger.LogInformation("{type} не найден. Создание файла по умолчанию {path}", typeof(T).Name, filePath);

        var defaults = Clone(createDefault());
        var json = JsonSerializer.Serialize(defaults, jsonTypeInfo);

        lock (stateLock)
        {
            _value = defaults;
            _persistedJson = json;
        }

        await WriteFileAsync(json);
    }

    private T Clone(T source)
    {
        var json = JsonSerializer.Serialize(source, jsonTypeInfo);
        return JsonSerializer.Deserialize(json, jsonTypeInfo)
               ?? throw new InvalidOperationException($"Не удалось скопировать {typeof(T).Name}.");
    }

    private async Task WriteFileAsync(string json)
    {
        var temporaryPath = $"{filePath}.tmp";
        await File.WriteAllTextAsync(temporaryPath, json);
        File.Move(temporaryPath, filePath, true);
    }

    private static bool ShouldCreateDirectory(string? directory)
        => directory is not null && !Directory.Exists(directory);
}
