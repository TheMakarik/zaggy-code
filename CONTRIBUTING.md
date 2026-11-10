# Contributing

Правила написания C#-кода и оформления коммитов в ZaggyCode.

## Главный принцип

Пиши современный, лаконичный и читаемый C#. При изменении существующего файла в первую очередь поддерживай его текущий стиль, а не ломай его ради формальных правил.

## Уточняющие вопросы

Задача считается **сложной**, если выполнено хотя бы одно условие:

- затрагивает несколько слоёв (`Core` + `Modules` + `Avalonia`) или переписывает существующий компонент с нуля;
- в требовании есть неоднозначности: цвета, размеры, отступы, имена, дефолты, UX, поведение на границах;
- пользователь сказал «не додумывай», «уточни», «посоветуйся»;
- в коде уже есть противоречия: отсутствуют ассеты, есть дубли или несоответствия.

**Ничего не додумывай.** Если данных не хватает — спроси. Не изобретай требования и не выбирай цвета, числа или имена «на глаз».

Процесс:

1. Сначала разведка: прочитай релевантный код, найди использования, проверь существование файлов, ассетов, опций и ресурсов. Вопросы должны быть предметными.
2. Задавай вопросы пакетом по 3–5 штук, для каждого — короткий заголовок и 2–4 варианта.
3. Рекомендуемый вариант ставь первым и помечай `(Рекомендуется)`.
4. Жди ответов до начала большой реализации. Мелкие безопасные шаги можно делать сразу.
5. Если по ходу всплыли новые неоднозначности — задай ещё пакет.

Обязательно уточняй:

- **Цвета** — точный `#RRGGBB`/`#AARRGGBB`, ресурс темы это или хардкод, какой теме должен соответствовать, есть ли альфа.
- **Размеры и геометрию** — толщины, радиусы, отступы, проценты: в DIP или в долях ячейки.
- **Дефолты** — что показывать или делать, если свойство не задано (`null`).
- **Поведение на границах** — пустые коллекции, отсутствующие файлы, `null`, превышение лимитов, отмена.
- **Имена** — файлы, типы, свойства, ключи ресурсов, XML-атрибуты.
- **Архитектуру** — где живёт логика (ViewModel / state machine / сервис), кто владеет данными.
- **Ассеты** — проверь, что все пути из `IOptions` реально существуют, сообщи о расхождениях и опечатках.

## Язык и платформа

- Целевая платформа — .NET (последняя стабильная LTS, поддерживаемая проектом).
- Используй возможности современного C#: file-scoped namespaces, primary constructors, pattern matching, switch expressions, null-conditional операторы, collection expressions, target-typed `new`.

## `var`

По умолчанию используй `var`, если тип очевиден из правой части выражения.

```csharp
// Хорошо
var user = await storage.LoadAsync();
var name = user.Name;
var sum = Calculate(a, b);
```

Исключения:

- **Collection expressions** — указывай явный тип коллекции.
- **Адаптация под файл** — если класс написан без `var`, не внедряй его насильно и не переписывай существующие строки.

## Проверка на `null`

Используй `is null` / `is not null` вместо `== null` / `!= null`.

```csharp
// Хорошо
if (value is null)
    return;

if (items is not null)
    Process(items);
```

## Защитные проверки

Не добавляй `ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrWhiteSpace` и подобные проверки без реальной необходимости. Проверка оправдана на публичной границе, где ошибка должна возникнуть в месте вызова, а не позже.

## Фигурные скобки и вложенность

- Если тело `if`/`else`/`for`/`while`/`foreach` состоит из одной строки, не оборачивай его в фигурные скобки.

```csharp
if (count > 0)
    Process(count);

foreach (var item in items)
    writer.Write(item);
```

- Избегай лишней вложенности: используй ранний `return`, `continue` и `break`.

```csharp
// Хорошо
if (value is null)
    return default;

return value.Transform();
```

## Порядок объявлений в классе

Придерживайся порядка: поля → конструкторы → свойства → события → методы. Primary constructor считается конструктором и находится в заголовке класса.

## Именование

- `PascalCase` — классы, интерфейсы, методы, свойства, публичные поля, enum-ы, namespace-ы.
- `camelCase` — локальные переменные, параметры, приватные поля.
- `_camelCase` — приватные поля класса.
- Интерфейсы начинаются с `I`.
- Асинхронные методы заканчиваются на `Async`.
- Не используй сокращения: `directory`, а не `dir`; `options`, а не `opts`; `buffer`, а не `buf`.
- Допускаются общепринятые аббревиатуры: `db`, `id`, `ui`.

## Модификаторы

- Явно указывай `private` для членов класса.
- Указывай `readonly` для полей, которые не изменяются после инициализации.
- Предпочитай `sealed` классам, которые не проектируются для наследования.

## Коллекции

### Тип в сигнатурах

Выбирай тип параметра или возврата по тому, что реально нужно потребителю:

- **Только чтение** (перебрать, посчитать, прочитать по индексу) — `IReadOnlyCollection<T>`, а если нужен доступ по индексу — `IReadOnlyList<T>`.
- **Только LINQ-цепочка** — `IEnumerable<T>`, но только если перечисление гарантированно одноразовое.
- **Возможно повторное перечисление** (несколько LINQ-операторов, `Count()`/`Any()` + `foreach`, повторный `foreach`) — `ICollection<T>`/`IReadOnlyCollection<T>` или один раз материализуй.
- **Изменение** (`Add`/`Remove`/`Clear`) — `ICollection<T>`/`IList<T>`/`IDictionary<TKey,TValue>` либо конкретный тип, если он нужен снаружи.

Не отдавай `List<T>`/`Dictionary<TKey,TValue>`, если потребителю достаточно интерфейса.

```csharp
// Хорошо — коллекция только читается
public IReadOnlyCollection<ThemeColorEntry> Colors { get; }

// Хорошо — одноразовое LINQ-перечисление
public IEnumerable<ThemeMetadata> Themes => _cache.Values;

// Хорошо — перечисление возможно несколько раз
public IReadOnlyCollection<ThemeMetadata> Themes { get; }

// Плохо — наружу отдаётся конкретный List без необходимости
public List<ThemeColorEntry> Colors { get; }
```

### Двойное перечисление

Если по `IEnumerable<T>` идёт больше одного прохода (несколько LINQ, `Any()` + `foreach`, `Count()` + доступ), материализуй один раз или прими `IReadOnlyCollection<T>`.

```csharp
// Плохо — source перечисляется дважды
if (source.Any())
    foreach (var item in source)
        Process(item);

// Хорошо
var items = source as IReadOnlyCollection<Item> ?? source.ToList();
if (items.Count > 0)
    foreach (var item in items)
        Process(item);
```

### Пустые коллекции и collection expressions

- Для пустых коллекций используй `[]` вместо `new List<T>()` и `Array.Empty<T>()`.
- Collection expressions (`[...]`) — исключение из правила `var`: указывай явный тип, чтобы читатель сразу видел вид коллекции.

```csharp
// Хорошо
int[] numbers = [1, 2, 3];
List<string> items = [];
Dictionary<int, string> map = [];
Span<char> buffer = ['a', 'b', 'c'];

// Плохо
var numbers = [1, 2, 3];
var items = new List<string>();
var map = new Dictionary<int, string>();
```

### Потокобезопасные коллекции

Обычные `List<T>`/`Dictionary<TKey,TValue>` не потокобезопасны. Если коллекция читается или пишется из нескольких потоков:

- `ConcurrentDictionary<TKey,TValue>`, `ConcurrentQueue<T>`, `ConcurrentBag<T>`, `ConcurrentStack<T>` — параллельный доступ без внешней блокировки.
- `ImmutableArray<T>`/`ImmutableDictionary<TKey,TValue>` — когда нужно отдавать неизменяемый снимок.
- `FrozenDictionary<TKey,TValue>`/`FrozenSet<T>` — справочники, созданные один раз и только читаемые (быстрее обычных).
- `BlockingCollection<T>`/`Channel<T>` — producer/consumer.

Не оборачивай обычную коллекцию в `lock`, если подходит concurrent/immutable.

```csharp
// Хорошо — параллельные записи
private readonly ConcurrentDictionary<string, ThemeMetadata> _themes = new(StringComparer.Ordinal);

// Плохо — lock поверх обычного словаря, когда есть ConcurrentDictionary
private readonly Dictionary<string, ThemeMetadata> _themes = [];
private readonly object _gate = new();
```

### LINQ

- Используй LINQ для декларативных преобразований, но не злоупотребляй: если запрос становится нечитаемым, разбей его или используй цикл.
- Материализуй (`ToList`/`ToArray`) осознанно: это один проход, но и аллокация.
- Не смешивай LINQ с побочными эффектами в `Select`.

### Чек-лист по коллекциям

- [ ] Тип параметра или возврата минимально достаточный: `IReadOnlyCollection`/`IReadOnlyList` для чтения, `ICollection`/`IList`/`IDictionary` для изменения.
- [ ] Нет двойного перечисления `IEnumerable<T>`.
- [ ] Пустые коллекции — `[]`, не `new List<T>()`/`Array.Empty<T>()`.
- [ ] Collection expressions — с явным типом.
- [ ] Для многопоточного доступа — concurrent/immutable/frozen коллекция, а не `lock` поверх `List<T>`.

## Асинхронность

- Всегда используй `async`/`await`.
- Избегай `.Result`, `.Wait()` и синхронных блокировок в асинхронном коде.
- Не лови исключения без причины. Если ловишь — обрабатывай, логируй или объясняй в комментарии, почему подавляешь.

## Комментарии

- Комментарии объясняют *почему*, а не *что*.
- Не оставляй закомментированный код.
- Не пиши XML-документацию.

## Структура файлов

- **Запрещено создавать вложенные классы** внутри других классов.
- **Запрещено размещать более одного публичного класса в одном файле.**
- Имя файла должно соответствовать имени публичного класса.
- Вспомогательные типы (`enum`, `record`, `struct`) допускаются в том же файле, только если они тесно связаны с основным классом и используются только им.
- В интерфейсах члены объявляются подряд, без пустых строк между методами, свойствами, событиями и другими объявлениями.

```csharp
// Хорошо
public interface IRobotController
{
    IRobotExecutor Executor { get; set; }
    void MoveUp();
    bool IsWallFromUp();
}

// Плохо
public interface IRobotController
{
    IRobotExecutor Executor { get; set; }

    void MoveUp();

    bool IsWallFromUp();
}
```

## Разделение ViewModel и View

- ViewModel не должна зависеть от View: не передавай в неё `Control`, `Window`, `Panel`, `Visual` и другие UI-типы Avalonia.
- Для коммуникации View → ViewModel используй `Interaction<TInput, TOutput>` из ReactiveUI.
- Для коммуникации ViewModel → ViewModel используй `MessageBus` из ReactiveUI.
- ViewModel может использовать `ICommand`, `Interaction` и сервисы, но не конкретные контролы.

## DI и IOptions

- Настройки биндятся из `appsettings.json` через `AddOptions<T>` в `Bootstrapper.cs`.

```csharp
services.AddOptions<StorageOptions>()
    .Bind(configuration.GetSection(nameof(StorageOptions)));
```

- Все свойства опций, содержащие пути, раскрывают переменные окружения через `Environment.ExpandEnvironmentVariables` в сеттере.
- В `appsettings.json` пути пишутся через переменные: `%ZAGGY_CONFIG%/state-settings.json`.
- `Bootstrapper.SetEnvironmentVariables()` создаёт недостающие директории до биндинга опций.
- Всегда используй `Path.Join` для склеивания сегментов пути, а не `Path.Combine`.

```csharp
public sealed class StorageOptions
{
    public required string DataPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
```

Переменные окружения:

| Переменная | Назначение |
|------------|-----------|
| `ZAGGY_APP` | Папка с приложением |
| `ZAGGY_CONFIG` | Пользовательские конфиги |
| `ZAGGY_STATE` | Логи и состояние |
| `ZAGGY_TEMP` | Временные файлы |

## Настройки приложения

### Главное правило

**Любая новая настройка, добавленная в окно настроек, обязана также появиться в верхнем меню приложения.** Пользователь должен иметь возможность изменить настройку, не открывая отдельное окно.

- Бинарная настройка — `MenuItem`-переключатель (`Theme="{StaticResource ZaggyToggleMenuItem}"`).
- Перечисление — подменю с `ItemsSource` через `EnumValuesConverter`, как у «Скорость» и «Положение боковой панели».

Проверяй оба меню в `MainWindow.axaml`: `full-menu` и `compact-menu` дублируют друг друга.

### Архитектура

```
StateSettings (Core) ──► IObservableStorage<StateSettings> ──► MainWindowViewModel
ViewSettings (Core)  ──► IObservableStorage<ViewSettings>  ──► MainWindowViewModel
     ▲                                                              │
     │                                                              ▼
SettingsViewModel ──► AppearanceSettingsViewModel            MessageBus
     │                                                              ▲
     └──────────────────── сохраняет + шлёт сообщение ──────────────┘
```

- **Модели**:
  - `src/desktop/ZaggyCode.Core/Data/Model/StateSettings.cs` — состояние пользователя: `LastLanguage`, `LastGamePath`, `LastSpeed`.
  - `src/desktop/ZaggyCode.Core/Data/Model/ViewSettings.cs` — настройки отображения: `UseSystemTitleBar`, `SidebarPosition`, `CodeFontSize`, `TerminalFontSize`, `CodeTheme`.
  Обе модели реализуют `INotifyPropertyChanged` через `SetField`.
- **Значения по умолчанию** — `appsettings.json` → `DefaultStateSettings.Settings` и `DefaultViewSettings.Settings`. Поле обязано называться так же, как свойство соответствующей модели.
- **Файлы данных** — `state-settings.json` и `view-settings.json` (пути в `StorageOptions`).
- **Окно настроек** — `SettingsWindow` → `SettingsViewModel` → `AppearanceSettingsViewModel` / `PythonSettingsViewModel` / `CSharpSettingsViewModel`. Каждая страница сама решает, что показывать.
- **Синхронизация между окнами** — через `MessageBus`. При сохранении `SettingsViewModel.SaveSettingsAsync` кладёт значения в `ViewSettings` **и** рассылает `...ChangedMessage`; `MainWindowViewModel` их слушает.

### Чек-лист: добавить настройку

1. **Модель** — добавь свойство в `StateSettings` (состояние) или `ViewSettings` (отображение) с `INotifyPropertyChanged` через `SetField`. Перечисления держи в отдельном файле в `ZaggyCode.Core.Data.Enums`.
2. **Default** — задай значение в `appsettings.json` → `DefaultStateSettings.Settings` или `DefaultViewSettings.Settings`.
3. **Окно настроек** — добавь поле в `AppearanceSettingsViewModel` (или другую страницу):
   - реактивное свойство и «оригинал» для отслеживания изменений (`_original...`);
   - включи `nameof(...)` в `WhenAnyPropertyChanged`, `LoadFromViewSettings`, `ResetToDefaults`, `AcceptChanges` и `UpdateHasChanges`.
4. **UI настроек** — добавь контрол во `AppearanceSettingsView.axaml` (или соответствующую вьюху).
5. **Верхнее меню** — добавь `MenuItem` в оба меню (`full-menu` **и** `compact-menu`) в `MainWindow.axaml`, привязав к команде или свойству `MainWindowViewModel`.
6. **ViewModel окна** — заведи реактивное свойство в `MainWindowViewModel`, инициализируй из `stateSettingsStorage.Current` или `viewSettingsStorage.Current`, подпишись на изменения и синхронизируй обратно.
7. **MessageBus** — если настройку можно менять из нескольких мест, заведи `...ChangedMessage` в `ViewModels/Messages`, слушай его в `MainWindowViewModel` и отправляй из `SettingsViewModel`.
8. **Сборка** — `dotnet build`.

Где что лежит:

- Сообщения — `src/desktop/ZaggyCode.Avalonia/ViewModels/Messages/`.
- Конвертеры для отображения enum в настройках — `src/desktop/ZaggyCode.Avalonia/Views/Converters/` (например `SidebarPositionToTextConverter`).
- Стили настроек — `src/desktop/ZaggyCode.Avalonia/Views/Styles/Settings/`.

## Цвета и темы

### Пайплайн цвета

`Theme` (Core) → `theme.xml` (внутри `.zct`) → `ThemeApplier` (рефлексия по строковым свойствам `Theme`) → `ThemeSetterProxy.SetColor` → ресурсы `{имя}Color` + `{имя}Brush`.

`ThemeApplier` отражает `typeof(Theme).GetProperties()`, где тип свойства — `string`, и вызывает `SetColor(resourceName, hex)`, отрезая у имени суффикс `Color`, если он есть.

Следствие для нейминга:

- свойство `MapWallColor` даёт ресурсы `MapWallColor`/`MapWallBrush`;
- свойство без суффикса `Color` (например `GoalColor1`) даёт ресурсы `GoalColor1Color`/`GoalColor1Brush`.

### Default-ресурсы

`src/desktop/ZaggyCode.Avalonia/Views/Styles/Colors.axaml` — это **клон темы Primus** (дефолтные значения до применения темы). Любой цвет, добавленный сюда, должен совпадать со значением Primus в `dev/scripts/generate-themes.cs`.

Добавляй пару ресурсов по конвенции:

```xml
<Color x:Key="FooColor">#RRGGBB</Color>
<SolidColorBrush Color="{DynamicResource FooColor}" x:Key="FooBrush" />
```

### Добавление нового цвета темы

1. **Модель** `src/desktop/ZaggyCode.Core/Theming/Model/Theme.cs` — объяви `required` строковое свойство и `[XmlElement("...")]`.
2. **Генератор** `dev/scripts/generate-themes.cs` — добавь параметр в `CreateTheme`, передай его в инициализатор `Theme`, затем задай значение для каждой из 4 тем (Primus, Tero, Viola, Brilla).
3. **Default-ресурсы** `Views/Styles/Colors/Colors.axaml` — добавь `<Color>` и `<SolidColorBrush>` с теми же значениями, что у Primus.
4. **Пересоздай архивы** `.zct`:

```bash
rm -f dev/themes/*.zct
dotnet run dev/scripts/generate-themes.cs
rm -f src/desktop/ZaggyCode.Avalonia/Themes/*.zct
cp dev/themes/*.zct src/desktop/ZaggyCode.Avalonia/Themes/
```

5. Обнови тесты, где создаётся `Theme` (`ThemeCreatorTests.CreateTheme`).
6. Проверь: `dotnet build` + профильные тесты (`ThemeCreatorTests`, `ThemeCopierTests`).

### Правила

- **Никогда не хардкодь цвета** в контролах и XAML — только `DynamicResource` и ресурсы темы.
- `Colors.axaml` перед добавлением сверяй с темой Primus, расхождения исправляй.
- `ThemeApplier` и `ThemeSetterProxy` менять не нужно: они сами разложат новое свойство `Theme` в ресурсы.
- Перед правкой `Colors.axaml` убедись, что он соответствует Primus.

### Структура архива темы

- `theme.xml` — сериализованный `Theme` (`XmlSerializer<Theme>`, корневой элемент `<theme>`).
- `meta.json` — `ThemeMetadata` для карточки темы в настройках (поля — превью-цвета).

Проверка содержимого: `tar -tjf src/desktop/ZaggyCode.Avalonia/Themes/Primus.zct` → внутри `meta.json` + `theme.xml`.

### Хранение и доступность

- `SystemThemesFolder` (`%ZAGGY_APP%/Themes`) — системные темы из сборки, `IsSystemTheme == true`.
- `ExternThemesFolder` (`%ZAGGY_DATA%/themes`) — пользовательские (скопированные), `IsSystemTheme == false`. Их можно удалять и редактировать.
- `ThemeCatalog` — перечисление и загрузка; `ThemeCopier` — копирование; `ThemeDeleter` — удаление (запрещает системные темы).

### Регистрация в DI и применение

- Сервисы `ThemeCatalog`/`ThemeCopier`/`ThemeDeleter` (Modules) попадают в DI автоматически через `Scrutor`-скан в `Bootstrapper.cs` как **Singleton** (`AsImplementedInterfaces`).
- Интерфейсы расположены в `ZaggyCode.Core.Theming.Interfaces`; `IThemeDeleter` подключается в `AppearanceSettingsViewModel` через цепочку `MainWindowViewModel → SettingsViewModel → AppearanceSettingsViewModel`.
- `ThemeApplier.ApplyThemeAsync(name)` загружает `.zct`, читает `Theme` и проставляет цветовые ресурсы. Если темы нет — фолбэк на `Primus` (`ThemeApplier.FallbackThemeName`) и показывается `ThemeNotFoundWindow`.

## XAML

### Структура UI-папок в ZaggyCode.Avalonia

```
Views/
├── Behaviors/          # Поведения для взаимодействия (TextEditorZoomBehavior, TerminalZoomBehavior)
├── Converters/         # Конвертеры как MarkupExtension
├── Controls/           # Кастомные контролы (MapView)
├── UserControls/       # Составные контролы
├── Styles/             # Стили: папка на контрол, файл на класс стиля
│   ├── Colors/         # Цветовые ресурсы (Colors.axaml)
│   ├── Common/         # База: окно, фоны, панели, карточки, подсказки
│   ├── Button/         # icon-button, run-button, primary-action и т.д.
│   ├── ComboBox/       # ComboBox, ComboBoxItem, language-selector
│   ├── Menu/           # MenuThemes, пункты меню, full/compact
│   ├── Editor/         # TextEditor, GridSplitter
│   ├── Flyout/         # презентеры попапов
│   ├── Settings/       # стили окна настроек
│   └── MainStyles.axaml  # StyleInclude всех файлов
├── Dialogs/            # Диалоговые окна
├── Utils/              # UI-утилиты
└── TerminalEngine/     # Реализация xterm движка (VirtualTerminal.Avalonia)
```

### Форматирование по XAML Styler

- Каждый атрибут — на новой строке.
- Порядок атрибутов: `x:Class`, `xmlns`, `x:DataType`, остальные атрибуты.

```xml
<rxui:ReactiveWindow
    x:Class="ZaggyCode.Avalonia.Views.MainWindow"
    x:DataType="vm:MainWindowViewModel"
    xmlns="https://github.com/avaloniaui"
    xmlns:rxui="http://reactiveui.net"
    xmlns:vm="using:ZaggyCode.Avalonia.ViewModels"
    xmlns:converters="clr-namespace:ZaggyCode.Avalonia.Views.Converters"
    Background="{DynamicResource BackgroundBrush}"
    Foreground="{DynamicResource ForegroundBrush}"
    Title="Программирование с Загги">
```

- `Grid` с определениями — одной строкой:

```xml
<Grid ColumnDefinitions="Auto,*,Auto" RowDefinitions="Auto,Auto,*">
```

- Элементы внутри `Grid` — атрибуты на отдельных строках:

```xml
<StackPanel
    Grid.Column="0"
    Grid.Row="0"
    Orientation="Horizontal"
    Spacing="12">
```

### Стили

Выноси повторяющуюся логику в стили:

```xml
<Style Selector="Button.icon-button">
    <Setter Property="Width" Value="32" />
    <Setter Property="Height" Value="32" />
    <Setter Property="Padding" Value="0" />
    <Setter Property="Background" Value="Transparent" />
</Style>

<Style Selector="Button.icon-button.window-control">
    <Setter Property="Width" Value="36" />
    <Setter Property="Height" Value="36" />
</Style>
```

Использование:

```xml
<Button Classes="icon-button run-button" Command="{Binding ExecuteCodeCommand}">
    <materialIcons:MaterialIcon Kind="Play" />
</Button>
```

### Behaviors

Сложную логику взаимодействия выноси в behaviors:

```xml
<avaloniaEdit:TextEditor>
    <Interaction.Behaviors>
        <behaviors:TextEditorZoomBehavior
            MaxFontSize="{Binding MaxFontSize}"
            MinFontSize="{Binding MinFontSize}"
            UpdateFontSizeCommand="{Binding UpdateFontSizeCommand}"
            ZoomStep="1" />
    </Interaction.Behaviors>
</avaloniaEdit:TextEditor>
```

### Адаптивность

Адаптивность делай через конвертеры:

```xml
<Menu IsVisible="{Binding Bounds.Width, Converter={converters:WidthToVisibilityConverter}, ConverterParameter='>=900'}">
    <!-- Полное меню -->
</Menu>

<Menu IsVisible="{Binding Bounds.Width, Converter={converters:WidthToVisibilityConverter}, ConverterParameter='<900'}">
    <!-- Компактное меню -->
</Menu>
```

### Конвертеры в XAML

Всегда используй конвертеры как `MarkupExtension`, без `StaticResource`:

```xml
<!-- Хорошо -->
<TextBlock IsVisible="{Binding ShowSidebar, Converter={converters:InverseBooleanConverter}}" />

<!-- Плохо -->
<TextBlock IsVisible="{Binding ShowSidebar, Converter={StaticResource InverseBooleanConverter}}" />
```

## Конвертеры Avalonia

- Конвертер реализуй как `MarkupExtension` + `IValueConverter`.
- Если `ConvertBack` не поддерживается, выбрасывай `NotSupportedException`.
- Название должно отражать действие: `BooleanToVisibilityConverter`, `EnumToStringConverter`.
- Суффикс `Converter` обязателен.
- Размещай конвертеры в `Views/Converters/`.

```csharp
public sealed class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return AvaloniaProperty.UnsetValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

Пример с enum:

```csharp
public sealed class PythonValidationResultToStringConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PythonFunctionNameValidationResult result)
            return null;

        return result switch
        {
            PythonFunctionNameValidationResult.Success => string.Empty,
            PythonFunctionNameValidationResult.Empty => "Имя функции не может быть пустым",
            PythonFunctionNameValidationResult.ContainsSpaces => "Имя функции не должно содержать пробелов",
            PythonFunctionNameValidationResult.ContainsForbiddenCharacters => "Имя функции содержит запрещённые символы",
            PythonFunctionNameValidationResult.StartsWithDigit => "Имя функции не может начинаться с цифры",
            PythonFunctionNameValidationResult.StartsWithUnderscore => "Имя функции не может начинаться с подчёркивания",
            PythonFunctionNameValidationResult.IsReservedGlobalFunction => "Имя зарезервировано для глобальной функции",
            PythonFunctionNameValidationResult.IsStandardLibraryModule => "Имя совпадает с модулем стандартной библиотеки",
            PythonFunctionNameValidationResult.IsStandardLibraryFunction => "Имя совпадает с функцией стандартной библиотеки",
            _ => "Некорректное имя функции"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

`IMultiValueConverter` для `MultiBinding`:

```csharp
public sealed class IsNotEqualConverter : MarkupExtension, IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
            return false;

        return !Equals(values[0], values[1]);
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

## Тестирование

Инструменты: xUnit, FakeItEasy, FluentAssertions, AutoFixture.

- Наследуй `IDisposable`, если нужна очистка.
- Поля зависимостей — вверху класса.
- Общие фейки инициализируй в конструкторе.
- Тестируемый сервис называй `systemUnderTests`.
- `A.Dummy<T>()` — для зависимостей, которые нужны конструктору, но не влияют на поведение.
- `A.Fake<T>()` — для зависимостей, чьё поведение настраивается.

```csharp
// Dummy — без настройки
var logger = A.Dummy<ILogger<ObservableStorage<ViewSettings>>>();

// Fake — с настройкой
var options = A.Fake<IOptions<StorageOptions>>();
A.CallTo(() => options.Value).Returns(new StorageOptions { ... });
```

- Генерируй данные через AutoFixture:

```csharp
var fixture = new Fixture();
var expectedFontSize = fixture.Create<int>();
var expectedTheme = fixture.Create<string>();
```

- Разделяй тест на блоки `// Arrange`, `// Act`, `// Assert`.
- Именование теста: `Действие_Условие_ОжидаемыйРезультат`.

```csharp
[Fact]
public async Task LoadAsync_WhenFileCorrupted_DeletesAndCreatesNewFile()
{
    // Arrange
    var systemUnderTests = CreateSystemUnderTests();

    // Act
    await systemUnderTests.LoadAsync();

    // Assert
    actualContent.Should().Contain(expectedViewSettings.CodeTheme);
}
```

- Утверждения — через FluentAssertions: `.Should().Be(...)`, `.Should().Contain(...)`, `.Should().NotBeNull()`.
- Избегай нескольких ассертов без веской причины.
- Для файловой системы используй `TestFileSystem` из `ZaggyCode.Tests.Infrastructure`.
- Пути в тестах формируй через `Path.Join`, а не `Path.Combine`.

## Файлы с `//#:NO_AI`

Файлы, у которых над объявлением класса есть директива `//#:NO_AI`, запрещено изменять по логике с помощью ИИ. Читать их можно.

Запрещено:

- менять логику, алгоритмы, поведение, тела методов, условия и порядок операций;
- переименовывать символы, типы, методы, параметры и поля;
- менять комментарии и форматирование, не связанные с перемещением файла.

Разрешено:

- перемещать файл или папку;
- обновлять `namespace` и `using`, если это следствие перемещения или реорганизации модулей;
- читать файл.

Если для задачи действительно нужны изменения логики такого файла — откажись работать с этим классом и не предлагай убирать `//#:NO_AI`.

## Процесс работы

1. Перед изменениями ознакомься с кодом вокруг места правки.
2. Убедись, что файл не помечен `//#:NO_AI`.
3. После изменений проверяй сборку (`dotnet build`).
4. Тесты запускай только когда менялись тестовые классы (`tests/**`).
5. Не оставляй закомментированный код, временные решения и плейсхолдеры.

## Коммиты

### Формат

```text
<Краткое описание изменений> (<Пометка>)
```

- Пиши кратко, что именно изменилось.
- Используй прошедшее время, как принято в репозитории: «Добавил», «Убрал», «Разделил», «Исправил».
- Без точки в конце.
- Один коммит — одно логическое изменение.
- Подробности, если нужны, пиши в теле коммита; пометка остаётся в конце заголовка.

### Пометка инструмента

В конце заголовка обязательно укажи, чем написан код:

- `(Написан руками)` — код написан человеком.
- `(Добавлен с помощью <ИИ>)` — код написан с помощью ИИ, с указанием конкретной модели.
- `(Добавлен с помощью <ИИ 1>, <ИИ 2>)` — если использовалось несколько ИИ.

Примеры:

```text
Добавил IRobotController и упростил IRobotExecutor (Добавлен с помощью DeepSeek V4.1 Flash)
```

```text
Разделил настройки на StateSettings и ViewSettings (Написан руками)
```

```text
Переписал рендер карты и добавил анимации (Добавлен с помощью DeepSeek V4.1 Flash, Claude Opus 4.1)
```

```text
Убрал пустые строки между членами интерфейсов (Написан руками)
```

Плохо:

```text
fix
```

```text
изменения
```

```text
Добавил IRobotController
```
