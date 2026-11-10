# Настройки приложения

## Главное правило
**Любая новая настройка, добавленная в окно настроек, обязана также появиться в верхнем меню приложения.**

Пользователь должен иметь возможность изменить настройку, не открывая отдельное окно. Если настройка бинарная —
добавь `MenuItem`-переключатель (`Theme="{StaticResource ZaggyToggleMenuItem}"`), если перечисление — подменю
со `ItemsSource` через `EnumValuesConverter`, как у «Скорость» и «Положение боковой панели».

> Это правило — часть контракта окна настроек, а не пожелание. Проверяй оба меню в `MainWindow.axaml`
> (`full-menu` и `compact-menu`) — они дублируют друг друга.

## Архитектура настроек

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
- **Значения по умолчанию** — `appsettings.json` → `DefaultStateSettings.Settings` и `DefaultViewSettings.Settings`.
  Поле обязано называться так же, как свойство соответствующей модели.
- **Файлы данных** — `state-settings.json` и `view-settings.json` (пути в `StorageOptions`).
- **Окно настроек** — `SettingsWindow` → `SettingsViewModel` → `AppearanceSettingsViewModel` /
  `PythonSettingsViewModel` / `CSharpSettingsViewModel`. Каждая страница сама решает, что показывать.
- **Синхронизация между окнами** — через `MessageBus`. При сохранении `SettingsViewModel.SaveSettingsAsync`
  кладёт значения в `ViewSettings` **и** рассылает `...ChangedMessage`; `MainWindowViewModel` их слушает.

## Чек-лист: добавить настройку

1. **Модель** — добавь свойство в `StateSettings` (состояние) или `ViewSettings` (отображение)
   с `INotifyPropertyChanged` через `SetField`. Перечисления держи в отдельном файле в `ZaggyCode.Core.Data.Enums`.
2. **Default** — задай значение в `appsettings.json` → `DefaultStateSettings.Settings` или `DefaultViewSettings.Settings`.
3. **Окно настроек** — добавь поле в `AppearanceSettingsViewModel` (или другую страницу):
   - реактивное свойство и «оригинал» для отслеживания изменений (`_original...`);
   - включи `nameof(...)` в `WhenAnyPropertyChanged`, `LoadFromViewSettings`, `ResetToDefaults`, `AcceptChanges`
     и `UpdateHasChanges`.
4. **UI настроек** — добавь контрол во `AppearanceSettingsView.axaml` (или соответствующую вьюху).
5. **Верхнее меню** — добавь `MenuItem` в оба меню (`full-menu` **и** `compact-menu`) в `MainWindow.axaml`,
   привязав к команде/свойству `MainWindowViewModel`. См. «Главное правило».
6. **ViewModel окна** — заведи реактивное свойство в `MainWindowViewModel`, инициализируй из
   `stateSettingsStorage.Current` или `viewSettingsStorage.Current`, подпишись на изменения и синхронизируй обратно.
7. **MessageBus** — если настройку можно менять из нескольких мест, заведи `...ChangedMessage`
   в `ViewModels/Messages`, слушай его в `MainWindowViewModel` и отправляй из `SettingsViewModel`.
8. **Сборка** — `dotnet build`.

## Где что лежит
- Сообщения: `src/desktop/ZaggyCode.Avalonia/ViewModels/Messages/`.
- Конвертеры для отображения enum в настройках: `src/desktop/ZaggyCode.Avalonia/Views/Converters/`
  (например `SidebarPositionToTextConverter`).
- Стили настроек: `src/desktop/ZaggyCode.Avalonia/Views/Styles/Settings/`.
