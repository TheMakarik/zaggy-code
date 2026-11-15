using System.Text.Json.Serialization.Metadata;

namespace ZaggyCode.Modules.Data;

//#:NO_AI
public sealed class SettingsStorage<T>(
    ILogger<SettingsStorage<T>> logger,
    string filePath,
    IBackgroundTaskHelper backgroundTaskHelper,
    BackgroundTaskKind backgroundKind,
    TimeSpan waitBeforeFlashInterval,
    JsonTypeInfo<T> jsonTypeInfo,
    Func<T> createDefault)
    : ISettingsStorage<T>
    where T : class, INotifyPropertyChanged
{
    private readonly ILogger _logger = logger;
    private readonly Lock _stateLock = new();
    private readonly Lock _flashLock = new();
    private readonly SemaphoreSlim _flushGate = new(1, 1);

    private T? _value;
    private string _persistedJson = string.Empty;
    private CancellationTokenSource? _pendingFlash;

    public T Value => _value ?? throw new InvalidOperationException($"Не удалось загрузить {typeof(T).Name}: значение отсутствует.");

    public bool HasChanges
    {
        get
        {
            lock (_stateLock)
                return !IsCurrentEqualToPersisted();
        }
    }

    public async Task LoadAsync()
    {
        _logger.LogInformation("Начало загрузки {type} из {path}", typeof(T).Name, filePath);

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
            _logger.LogError(e, "Ошибка загрузки {type} из {path}", typeof(T).Name, filePath);
            File.Delete(filePath);
            await CreateDefaultFileAsync();
        }
    }

    public void Revert()
    {
        string baseline;
        lock (_stateLock)
            baseline = _persistedJson;

        if (baseline.Length == 0)
            return;

        _logger.LogDebug("Отмена изменений {type} до сохранённого состояния", typeof(T).Name);

        var restored = JsonSerializer.Deserialize(baseline, jsonTypeInfo);
        if (restored is null)
            return;

        SettingsCopier.Copy(restored, Value);
        _logger.LogInformation("Изменения {type} отменены до сохранённого состояния", typeof(T).Name);
    }

    public async ValueTask FlushAsync()
    {
        await _flushGate.WaitAsync();
        try
        {
            string json;
            lock (_stateLock)
                json = JsonSerializer.Serialize(Value, jsonTypeInfo);

            await WriteFileAsync(json);

            lock (_stateLock)
                _persistedJson = json;

            _logger.LogInformation("Файл {type} успешно сохранён по пути {path}", typeof(T).Name, filePath);
        }
        finally
        {
            _flushGate.Release();
        }
    }

    public async ValueTask ToDefaultAsync()
    {
        var defaults = Clone(createDefault());
        SettingsCopier.Copy(defaults, Value);
        await FlushAsync();
        _logger.LogInformation("{type} сброшены к значениям по умолчанию", typeof(T).Name);
    }

    public void FlashAtBackground(bool waitBeforeFlash = false)
    {
        _logger.LogDebug(
            "Фоновое сохранение {type}: ожидание перед сохранением — {wait}",
            typeof(T).Name,
            waitBeforeFlash);

        backgroundTaskHelper.StartBackground(new BackgroundTaskStartDto
        {
            Kind = backgroundKind,
            HasNoProgress = true,
            DelegateToStart = _ => FlashCoreAsync(waitBeforeFlash)
        });
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
        lock (_flashLock)
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
            _logger.LogError(e, "Не удалось сохранить {type} в {path}", typeof(T).Name, filePath);
        }
    }

    private bool IsCurrentEqualToPersisted()
        => JsonSerializer.Serialize(Value, jsonTypeInfo) == _persistedJson;

    private void StoreLoaded(T loaded)
    {
        var json = JsonSerializer.Serialize(loaded, jsonTypeInfo);
        lock (_stateLock)
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

        _logger.LogInformation("{type} не найден. Создание файла по умолчанию {path}", typeof(T).Name, filePath);

        var defaults = Clone(createDefault());
        var json = JsonSerializer.Serialize(defaults, jsonTypeInfo);

        lock (_stateLock)
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
