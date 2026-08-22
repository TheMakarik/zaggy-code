# Упрощение связей View ↔ ViewModel + переход на IGameEngine

## Контекст проблемы

Сейчас в `MainWindow` связи View↔ViewModel запутанные:
- View **проталкивает состояние** в VM публичными сеттерами: `ViewModel.TerminalReader = ...`, `TerminalWriter = ...`, `Executor = GameMap.Executor`.
- View **напрямую дёргает** interaction VM: `Terminal.PropertyChanged += ... ViewModel?.ResizeGridToMax.Handle(...)`.
- View **подписан на MessageBus** (`FontSizeToastMessage`, `CodeThemeChangedMessage`) — по AGENTS.md MessageBus только для VM↔VM.
- VM сама резолвит keyed `ILanguageRunner` через `IServiceScopeFactory`, хотя пользователь переводит систему на `IGameEngine`.
- Баг в VM: `this.WhenAnyPropertyChanged().Subscribe(...)` **пересоздаёт все вложенные `WhenAnyValue`-подписки при изменении любого свойства** (утечка подписок).

## Изменения

### 1. `ViewModels/MainWindowViewModel.cs`

**Переход на IGameEngine** (в `Bootstrapper` уже регистрируется как singleton — правки DI не нужны):
- Убрать `IServiceScopeFactory _factory`; внедрить `IGameEngine _gameEngine` через конструктор.
- Удалить свойства `IRobotExecutor? Executor`, `TextReader? TerminalReader`, `TextWriter? TerminalWriter` и мёртвое `MapAssets` (нигде не читается — проверено grep'ом).
- Добавить interaction `Interaction<Unit, (TextReader Input, TextWriter Output)> GetTerminalStreams` — View отдаёт потоки терминала по запросу.
- `RunCode()` упрощается: получить код (`GetCodeToExecute`) → получить потоки (`GetTerminalStreams`) → выставить `engine.Language/Speed/Input/Output` → подписаться на `DebugLineUpdated`/`CodeErrorOccurred` → `await engine.RunCode(code, token)` → отписаться в `finally`.
- `OnCodeErrorOccurred` теперь только логирует (движок, как и раннеры, сам пишет ошибки в свой `Output`) — убрать поле `_codeErrorText`.
- `#if DEBUG SelectedLanguage = Language.Python` — сохранить как есть.

**Тосты через Interaction вместо MessageBus:**
- Добавить `Interaction<string, Unit> ShowToast`. Команды шрифтов становятся `async Task` и шлют туда готовый текст (формат тот же: «Размер шрифта редактора изменён на N»).
- Подписка на `FontSizeToastMessage` остаётся в VM (SettingsViewModel его шлёт — это VM↔VM), но перенаправляет в `ShowToast`.

**Тема подсветки:** в подписке VM на `CodeThemeChangedMessage` дополнительно дёргать новый `Interaction<string, Unit> ApplyCodeTheme` — View применяет тему только через interaction.

**Фикс бага подписок:** заменить вложенные `WhenAnyPropertyChanged().Subscribe(...)` на прямые `WhenAnyValue(...).Subscribe(...)` по каждому свойству (один раз в конструкторе).

### 2. `Views/MainWindow.axaml.cs`

- Разбить гигантский `DataContextChanged`-лямбда-блок на именованные методы: `RegisterInteractionHandlers(viewModel)`, `MaximizeTerminalArea()`, `RestoreGridArea()`.
- Зарегистрировать новые interactions: `GetTerminalStreams` (возвращает `(_terminalSession.Reader, _terminalSession.Writer)`), `ShowToast` (вызывает `ShowFontSizeToast`), `ApplyCodeTheme` (вызывает существующий `ApplyCodeTheme`).
- Удалить обе подписки View на `MessageBus` (`FontSizeToastMessage`, `CodeThemeChangedMessage`) — View общается с VM только через Interaction.
- Удалить присваивания `ViewModel.TerminalReader/Writer/Executor`.
- `Terminal.PropertyChanged` больше не зовёт `ViewModel?.ResizeGridToMax.Handle(...)` — терминал, уменьшенный драгом до минимума, разворачивается локальным вызовом `MaximizeTerminalArea()` (чистая view-логика layout'а).
- Удалить никогда не устанавливаемое поле `_isTerminalMaximized`.
- Остальные handlers (`GetCodeToExecute`, `UpdateCodeLine`, `StopCodeExecution`, `ResetMap`, `ConcludeRun`, `ClearTerminalContent`, `OpenSettings`, `ResizeGridToMax`, `BackGridToNormal`) и логика тостов/тем/ресайза остаются как есть.

### 3. Без изменений

- `FontSizeToastMessage` и остальные сообщения **сохраняются** (это VM↔VM канал: Settings → Main).
- `SettingsViewModel`/`SettingsWindow` уже построены на Interaction — не трогаем.
- XAML не меняется (удаляемые свойства нигде не забиндены).
- `IGameEngine`, `GameEngine`, раннеры — все под `//#:NO_AI`, не трогаем.

## Важное следствие

`GameEngine.RunCode` сейчас — WIP-заглушка (ждёт три фоновые задачи и завершается), поэтому после этого рефакторинга кнопка «Запустить» **не будет выполнять код**, пока ты не допишешь `GameEngine` (он сам будет резолвить раннеры и робота). Это ожидаемо: ты явно попросил заменить `ILanguageRunner` на `IGameEngine`.

## Проверка

- `dotnet build` (в Avalonia-проекте `WarningsAsErrors=true` — асинхронные лямбды оформлю в стиле файла с прагмами).
- `dotnet test` — существующие тесты не задеты, но проверю сборку целиком.