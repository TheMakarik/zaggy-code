
# IOptions Configuration

## Биндинг конфигурации
Настройки биндятся из `appsettings.json` через `AddOptions<T>` в `Bootstrapper.cs`

```csharp
// В Bootstrapper.cs
services.AddOptions<StorageOptions>()
    .Bind(configuration.GetSection(nameof(StorageOptions)));
```

## Раскрытие переменных окружения
Все свойства опций, содержащие пути, должны раскрывать переменные окружения через `Environment.ExpandEnvironmentVariables` в сеттерах (C# 14 `field` keyword).

```csharp
public class StorageOptions
{
    private string _dataPath = string.Empty;
    
    public string DataPath
    {
        get => _dataPath;
        set => _dataPath = Environment.ExpandEnvironmentVariables(value);
    }
}
```

## Правила
- В `appsettings.json` пути пишутся через переменные: `%ZAGGY_CONFIG%/data.json`
- `Bootstrapper.SetEnvironmentVariables()` создаёт недостающие директории перед биндингом
- Всегда используй `Path.Join` для склеивания сегментов пути в опциях

## Переменные окружения
| Переменная | Назначение |
|------------|-----------|
| `ZAGGY_APP` | Папка с приложением |
| `ZAGGY_CONFIG` | Пользовательские конфиги |
| `ZAGGY_STATE` | Логи и состояние |
| `ZAGGY_TEMP` | Временные файлы |

## Регистрация в DI
```csharp
// В Bootstrapper.cs
services.AddSingleton(typeof(IOptions<>), typeof(OptionsManager<>));
```
