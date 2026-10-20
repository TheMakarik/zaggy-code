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
UserData (Core)  ──►  IObservableStorage<UserData>  ──►  MainWindowViewModel
     ▲                                                          │
     │                                                          ▼
SettingsViewModel ──► AppearanceSettingsViewModel         MessageBus
     │                                                          ▲
     └──────────────────── сохраняет + шлёт сообщение ───────────┘
```

- **Модель** — `src/desktop/ZaggyCode.Core/Data/Model/UserData.cs`. Настройки-значения храни, по возможности,
  в `UserData`. Изменения пишутся в хранилище из `MainWindowViewModel` при изменении реактивных свойств.
- **Значения по умолчанию** — `appsettings.json` → секция `DefaultUser.User`. Поле обязано называться так же,
  как свойство `UserData`.
- **Окно настроек** — `SettingsWindow` → `SettingsViewModel` → `AppearanceSettingsViewModel` /
  `PythonSettingsViewModel` / `CSharpSettingsViewModel`. Каждая страница сама решает, что показывать.
- **Синхронизация между окнами** — через `MessageBus`. При сохранении `SettingsViewModel.SaveSettingsAsync`
  кладёт значения в `UserData` **и** рассылает `...ChangedMessage`; `MainWindowViewModel` их слушает.

## Чек-лист: добавить настройку

1. **Модель** — добавь свойство в `UserData` (если это значение пользователя) с `INotifyPropertyChanged`
   через `SetField`. Перечисления держи в отдельном файле в `ZaggyCode.Core.Data.Enums`.
2. **Default** — задай значение в `appsettings.json` → `DefaultUser.User`.
3. **Окно настроек** — добавь поле в `AppearanceSettingsViewModel` (или другую страницу):
   - реактивное свойство и «оригинал» для отслеживания изменений (`_original...`);
   - включи `nameof(...)` в `WhenAnyPropertyChanged`, `LoadFromUserData`, `ResetToDefaults`, `AcceptChanges`
     и `UpdateHasChanges`.
4. **UI настроек** — добавь контрол во `AppearanceSettingsView.axaml` (или соответствующую вьюху).
5. **Верхнее меню** — добавь `MenuItem` в оба меню (`full-menu` **и** `compact-menu`) в `MainWindow.axaml`,
   привязав к команде/свойству `MainWindowViewModel`. См. «Главное правило».
6. **ViewModel окна** — заведи реактивное свойство в `MainWindowViewModel`, инициализируй из
   `userStorage.Current`, подпишись на изменения `UserData` и синхронизируй обратно в хранилище.
7. **MessageBus** — если настройку можно менять из нескольких мест, заведи `...ChangedMessage`
   в `ViewModels/Messages`, слушай его в `MainWindowViewModel` и отправляй из `SettingsViewModel`.
8. **Сборка** — `dotnet build`.

## Где что лежит
- Сообщения: `src/desktop/ZaggyCode.Avalonia/ViewModels/Messages/`.
- Конвертеры для отображения enum в настройках: `src/desktop/ZaggyCode.Avalonia/Views/Converters/`
  (например `SidebarPositionToTextConverter`).
- Стили настроек: `src/desktop/ZaggyCode.Avalonia/Views/Styles/Settings/`.
