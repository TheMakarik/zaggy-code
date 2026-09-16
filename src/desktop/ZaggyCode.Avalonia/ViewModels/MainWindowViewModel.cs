namespace ZaggyCode.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _isTerminalVisible = true;
    [Reactive] private bool _isTerminalExists = true;
    [Reactive] private bool _isRunning = false;
    [Reactive] private bool _useOsDecoration = false;
    [Reactive] private bool _showSidebar = true;
    [Reactive] private bool _enableCodeHighlighting = true;
    [Reactive] private bool _showCodeLineNumbers = true;
    [Reactive] private ExecutionSpeed _executionSpeed;
    [Reactive] private Language _selectedLanguage;
    [Reactive] private LanguageItem? _selectedLanguageItem;
    [Reactive] private Game _currentGame;
    [Reactive] private Map _currentMap;
    [Reactive] private int _textEditorFontSize;
    [Reactive] private int _terminalFontSize;

    #endregion

    #region Properties

    public int MaxFontSize { get; init; }
    public int MinFontSize { get; init; }
    public string CodeTheme { get; private set; }
    public PopupOptions PopupOptions { get; }
    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public ObservableCollection<LanguageItem> AvailableLanguages { get; } = [];
    public ObservableCollection<MapNumberItem> MapNumbers { get; } = [];
    public IOptions<ZaggyAssetsOptions> ZaggyAssets { get; init; }

    public string CoinAssetPath => _mapAssetsOptions.Value.Coin;
    public string ZaggyFrontPath => ZaggyAssets.Value.ZaggySideFront;
    public string ZaggyBackPath => ZaggyAssets.Value.ZaggySideBack;
    public string ZaggyLeftPath => ZaggyAssets.Value.ZaggySideLeft;
    public string ZaggyRightPath => ZaggyAssets.Value.ZaggySideRight;
    public string ZaggyFrontLaggyPath => ZaggyAssets.Value.ZaggySideFrontLaggy;
    public string ZaggyBackLaggyPath => ZaggyAssets.Value.ZaggySideBackLaggy;
    public string ZaggyLeftLaggyPath => ZaggyAssets.Value.ZaggySideLeftLaggy;
    public string ZaggyRightLaggyPath => ZaggyAssets.Value.ZaggySideRightLaggy;
    public string ZaggySadPath => ZaggyAssets.Value.EmotionSad;
    public double MoveStepSeconds => _animationOptions.Value.MoveStepSeconds;
    public double SadSeconds => _animationOptions.Value.SadSeconds;

    #endregion

    #region Interaction

    public readonly Interaction<Unit, Unit> ResizeGridToMax = new();
    public readonly Interaction<Unit, Unit> ClearTerminalContent = new();
    public readonly Interaction<Unit, Unit> BackGridToNormal = new();
    public readonly Interaction<Unit, string> GetCodeToExecute = new();
    public readonly Interaction<Unit, string> GetSelectedCode = new();
    public readonly Interaction<Unit, (TextReader Input, TextWriter Output)> GetTerminalStreams = new();
    //public readonly Interaction<Unit, Map?> GetGameMap = new();
    public readonly Interaction<int, Unit> UpdateCodeLine = new();
    public readonly Interaction<Unit, Unit> StopCodeExecution = new();
    public readonly Interaction<Unit, Unit> ResetMap = new();
    public readonly Interaction<Unit, Unit> ConcludeRun = new();
    public readonly Interaction<string, Unit> ShowToast = new();
    public readonly Interaction<string, Unit> ApplyCodeTheme = new();
    public readonly Interaction<SettingsViewModel, Unit> OpenSettings = new();
    public readonly Interaction<Unit, Unit> OpenAbout = new();
    public readonly Interaction<Unit, Unit> OpenAboutAvalonia = new();

    #endregion

    #region Services

    private readonly IRobotGameEngine _robotGameEngine;
    private readonly IObservableStorage<UserData> _userStorage;
    private readonly IObservableStorage<ThemeSettings> _themeSettingsStorage;
    private readonly IObservableStorage<PythonSettings> _pythonSettingsStorage;
    private readonly IObservableStorage<CSharpSettings> _csharpSettingsStorage;
    private readonly IThemeCatalog _themeCatalog;
    private readonly IOptions<LoadingOptions> _loadingOptions;
    private readonly IOptions<DefaultUser> _defaultUserOptions;
    private readonly IOptions<DefaultThemeSettings> _defaultThemeSettingsOptions;
    private readonly IOptions<PythonDefaultSettingsOptions> _pythonDefaultSettingsOptions;
    private readonly IOptions<CSharpDefaultSettingsOptions> _csharpDefaultSettingsOptions;
    private readonly IOptions<CodeExamplePathOptions> _codeExamplePathOptions;
    private readonly IThemeCopier _themeCopier;
    private readonly IThemeEditorService _themeEditor;
    private readonly IThemeCreator _themeCreator;
    private readonly IOptions<ThemeOptions> _themeOptions;
    private readonly IOptions<FontSizeOptions> _fontSizeOptions;
    private readonly IOptions<CodeThemeDisplayNameOptions> _displayNameOptions;
    private readonly IOptions<CodeThemeIconOptions> _iconOptions;
    private readonly IPythonFunctionNameValidator _pythonFunctionNameValidator;
    private readonly IThemeDeleter _themeDeleter;
    private readonly IOptions<MapAssetsOptions> _mapAssetsOptions;
    private readonly IOptions<MapAnimationOptions> _animationOptions;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly object _executionLock = new();
    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Constructors

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IRobotGameEngine robotGameEngine,
        IOptions<ZaggyAssetsOptions> zaggyAssets,
        IObservableStorage<UserData> userStorage,
        IObservableStorage<ThemeSettings> themeSettingsStorage,
        IObservableStorage<PythonSettings> pythonSettingsStorage,
        IObservableStorage<CSharpSettings> csharpSettingsStorage,
        IOptions<CSharpDefaultSettingsOptions> csharpDefaultSettingsOptions,
        IThemeCatalog themeCatalog,
        IThemeCopier themeCopier,
        IThemeEditorService themeEditor,
        IThemeCreator themeCreator,
        IOptions<ThemeOptions> themeOptions,
        IOptions<LoadingOptions> loadingOptions,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<DefaultThemeSettings> defaultThemeSettingsOptions,
        IOptions<PythonDefaultSettingsOptions> pythonDefaultSettingsOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<PopupOptions> popupOptions,
        IOptions<FontSizeOptions> textFontSize,
        IOptions<CodeThemeDisplayNameOptions> displayNameOptions,
        IOptions<CodeThemeIconOptions> iconOptions,
        IPythonFunctionNameValidator pythonFunctionNameValidator,
        IThemeDeleter themeDeleter,
        IOptions<MapAssetsOptions> mapAssetsOptions,
        IOptions<MapAnimationOptions> animationOptions,
        ILoggerFactory loggerFactory)
    {
        _robotGameEngine = robotGameEngine;
        ZaggyAssets = zaggyAssets;
        _logger = logger;
        _fontSizeOptions = textFontSize;
        _userStorage = userStorage;
        _themeSettingsStorage = themeSettingsStorage;
        _pythonSettingsStorage = pythonSettingsStorage;
        _csharpSettingsStorage = csharpSettingsStorage;
        _themeCatalog = themeCatalog;
        _themeCopier = themeCopier;
        _themeEditor = themeEditor;
        _themeCreator = themeCreator;
        _themeOptions = themeOptions;
        _loadingOptions = loadingOptions;
        _defaultUserOptions = defaultUserOptions;
        _defaultThemeSettingsOptions = defaultThemeSettingsOptions;
        _pythonDefaultSettingsOptions = pythonDefaultSettingsOptions;
        _csharpDefaultSettingsOptions = csharpDefaultSettingsOptions;
        _codeExamplePathOptions = codeExamplePathOptions;
        _displayNameOptions = displayNameOptions;
        _iconOptions = iconOptions;
        _pythonFunctionNameValidator = pythonFunctionNameValidator;
        _themeDeleter = themeDeleter;
        _mapAssetsOptions = mapAssetsOptions;
        _animationOptions = animationOptions;
        _loggerFactory = loggerFactory;
        _executionSpeed = userStorage.Current.LastSpeed;
        _selectedLanguage = userStorage.Current.LastLanguage;
        _textEditorFontSize = userStorage.Current.CodeFontSize;
        _terminalFontSize = userStorage.Current.TerminalFontSize;
        _useOsDecoration = userStorage.Current.UseSystemTitleBar;
        _showSidebar = userStorage.Current.ShowSidebar;
        _enableCodeHighlighting = userStorage.Current.EnableCodeHighlighting;
        _showCodeLineNumbers = userStorage.Current.ShowCodeLineNumbers;
        CodeTheme = userStorage.Current.CodeTheme;
        PopupOptions = popupOptions.Value;
        MaxFontSize = _fontSizeOptions.Value.MaxFontSize;
        MinFontSize = _fontSizeOptions.Value.MinFontSize;
        CurrentGame = CreateTestGame();
        Debug.Assert(CurrentGame.Maps.Count <= MaxMaps, "Не более 9 карт");

        for (var index = 0; index < CurrentGame.Maps.Count; index++)
            MapNumbers.Add(new MapNumberItem(index + 1));

        CurrentMap = CurrentGame.Maps[0];
        MapNumbers[0].IsSelected = true;

        InitializeMessageBusSubscriptions();
        InitializeAvailableCodeThemes();
        InitializeAvailableLanguages();
        InitializeLanguageSelection();
        InitializeStorageSynchronization();
        InitializeGameEngine();

#pragma warning disable AsyncVoidMethod
        this.WhenAnyValue(vm => vm.IsTerminalVisible)
            .Skip(1)
            .Where(isVisible => !isVisible)
            .Subscribe(async void (_) => await ResizeGridToMax.Handle(Unit.Default));

        this.WhenAnyValue(vm => vm.IsTerminalVisible)
            .Skip(1)
            .Where(isVisible => isVisible)
            .Subscribe(async void (_) => await BackGridToNormal.Handle(Unit.Default));

        this.WhenAnyValue(vm => vm.IsTerminalExists)
            .Skip(1)
            .Where(isTerminalExists => !isTerminalExists)
            .Subscribe(async void (_) => await ClearTerminalContent.Handle(Unit.Default));
#pragma warning restore AsyncVoidMethod
    }

    private static readonly int[] TestMapSizes = [3, 4, 5];
    private const int MaxMaps = 9;

    // Тестовая игра: три карты, require-draw образует букву Г разных размеров.
    public static Game CreateTestGame()
    {
        ObservableCollection<Map> maps = [.. TestMapSizes.Select(CreateLetterMap)];

        return new Game
        {
            Name = "Тестовая игра",
            Description = """
                # Тестовая игра

                Добро пожаловать в **тестовую игру**! Управляй роботом **Загги** и закрась букву **Г**.

                ## Цель
                - Закрась все клетки буквы **Г** — они помечены как *require-draw*.
                - Собери **монетки** в тайной комнате за стеной: там закраска не требуется.
                - Доведи Загги до **финиша** — клетка в шахматку.

                ## Подсказки
                > Если Загги врежется в стену, он загрустит и вернётся назад.

                1. Сначала пройди по вертикали вниз.
                2. Затем поверни и пройди по горизонтали.

                | Клетка | Значение |
                |--------|----------|
                | `require-draw` | нужно закрасить |
                | монетка | собрать |
                | финиш | завершить уровень |

                Удачи!
                """,
            Author = "TheMakarik",
            Difficulty = Difficulty.VeryEasy,
            Maps = maps
        };
    }

    private static Map CreateLetterMap(int size)
    {
        const int cornerRadius = 14;

        ObservableCollection<GamePoint> points = [];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
                points.Add(new GamePoint { X = x, Y = y, RequireDraw = x == 0 || y == 0 });
        }

        points.First(point => point.X == 0 && point.Y == size - 1).IsSpawn = true;

        var goal = points.First(point => point.X == size - 1 && point.Y == 0);
        goal.IsGoal = true;
        goal.MakeRequireDrawOnGoalBold = true;

        // Сплошная стена буквой Г: колонка 1 (снизу оставлен проход) + строка 1.
        for (var y = 1; y < size - 1; y++)
            points.First(point => point.X == 1 && point.Y == y).WallType = WallType.Full;

        for (var x = 2; x < size; x++)
            points.First(point => point.X == x && point.Y == 1).WallType = WallType.Full;

        // Монетки в скрытой комнате за стеной (закраска там не требуется).
        points.First(point => point.X == size - 1 && point.Y == size - 1).HasCoin = true;
        if (size > 3)
            points.First(point => point.X == 2 && point.Y == 2).HasCoin = true;

        // На большой карте — боковая стеночка между монетками.
        if (size > 4)
            points.First(point => point.X == 3 && point.Y == 3).WallType = WallType.BottomRight;

        // Немного скруглить внешние углы карты.
        points.First(point => point.X == 0 && point.Y == 0).CustomBorderRadiusLeftTop = cornerRadius;
        points.First(point => point.X == size - 1 && point.Y == 0).CustomBorderRadiusRightTop = cornerRadius;
        points.First(point => point.X == 0 && point.Y == size - 1).CustomBorderRadiusLeftBottom = cornerRadius;
        points.First(point => point.X == size - 1 && point.Y == size - 1).CustomBorderRadiusRightBottom = cornerRadius;

        return new Map
        {
            Width = size,
            Height = size,
            Points = points
        };
    }

    private void InitializeAvailableCodeThemes()
    {
        foreach (var property in typeof(CodeThemeDisplayNameOptions).GetProperties())
        {
            var themeName = property.Name;
            var displayName = (string?)property.GetValue(_displayNameOptions.Value) ?? themeName;

            var iconProperty = typeof(CodeThemeIconOptions).GetProperty(themeName);
            var iconName = (string?)iconProperty?.GetValue(_iconOptions.Value);
            var iconKind = Enum.TryParse<MaterialIconKind>(iconName, out var kind) ? kind : MaterialIconKind.Palette;

            AvailableCodeThemes.Add(new CodeThemeItem(themeName, displayName, iconKind));
        }
    }

    private void InitializeAvailableLanguages()
    {
        foreach (var language in Enum.GetValues<Language>())
            AvailableLanguages.Add(new LanguageItem(language, language.GetPrettyName(), GetLanguageIcon(language)));
    }

    private static MaterialIconKind GetLanguageIcon(Language language) => language switch
    {
        Language.CSharp => MaterialIconKind.LanguageCsharp,
        Language.Python => MaterialIconKind.LanguagePython,
        _ => MaterialIconKind.Code
    };

    private void InitializeLanguageSelection()
    {
        this.WhenAnyValue(vm => vm.SelectedLanguage)
            .Subscribe(language => SelectedLanguageItem =
                AvailableLanguages.FirstOrDefault(item => item.Value == language));

        this.WhenAnyValue(vm => vm.SelectedLanguageItem)
            .Where(item => item is not null && item.Value != SelectedLanguage)
            .Subscribe(item => SelectedLanguage = item!.Value);
    }

    private void InitializeMessageBusSubscriptions()
    {
        MessageBus.Current.Listen<CodeThemesRequestMessage>()
            .Subscribe(_ => MessageBus.Current.SendMessage(new CodeThemesResponseMessage(AvailableCodeThemes)));

        MessageBus.Current.Listen<CodeFontSizeChangedMessage>()
            .Subscribe(message => TextEditorFontSize = message.FontSize);

        MessageBus.Current.Listen<TerminalFontSizeChangedMessage>()
            .Subscribe(message => TerminalFontSize = message.FontSize);

        MessageBus.Current.Listen<UseSystemTitleBarChangedMessage>()
            .Subscribe(message => UseOsDecoration = message.UseSystemTitleBar);

        MessageBus.Current.Listen<ShowSidebarChangedMessage>()
            .Subscribe(message => ShowSidebar = message.ShowSidebar);

        MessageBus.Current.Listen<EnableCodeHighlightingChangedMessage>()
            .Subscribe(message => EnableCodeHighlighting = message.EnableCodeHighlighting);

        MessageBus.Current.Listen<ShowCodeLineNumbersChangedMessage>()
            .Subscribe(message => ShowCodeLineNumbers = message.ShowCodeLineNumbers);

#pragma warning disable AsyncVoidMethod
        MessageBus.Current.Listen<CodeThemeChangedMessage>()
            .Subscribe(async void (message) =>
            {
                _userStorage.Current.CodeTheme = message.ThemeName;
                CodeTheme = message.ThemeName;
                await ApplyCodeTheme.Handle(message.ThemeName);
            });

        MessageBus.Current.Listen<FontSizeToastMessage>()
            .Subscribe(async void (message) => await ShowToast.Handle($"Размер шрифта {message.Source} изменён на {message.FontSize}"));
#pragma warning restore AsyncVoidMethod
    }

    private void InitializeStorageSynchronization()
    {
        this.WhenAnyValue(vm => vm.TextEditorFontSize)
            .Where(size => size != _userStorage.Current.CodeFontSize)
            .Subscribe(_ => _userStorage.Current.CodeFontSize = _textEditorFontSize);

        this.WhenAnyValue(vm => vm.TerminalFontSize)
            .Where(size => size != _userStorage.Current.TerminalFontSize)
            .Subscribe(_ => _userStorage.Current.TerminalFontSize = _terminalFontSize);

        this.WhenAnyValue(vm => vm.UseOsDecoration)
            .Where(useOsDecoration => useOsDecoration != _userStorage.Current.UseSystemTitleBar)
            .Subscribe(_ => _userStorage.Current.UseSystemTitleBar = _useOsDecoration);

        this.WhenAnyValue(vm => vm.ShowSidebar)
            .Where(showSidebar => showSidebar != _userStorage.Current.ShowSidebar)
            .Subscribe(_ => _userStorage.Current.ShowSidebar = _showSidebar);

        this.WhenAnyValue(vm => vm.EnableCodeHighlighting)
            .Where(enable => enable != _userStorage.Current.EnableCodeHighlighting)
            .Subscribe(_ => _userStorage.Current.EnableCodeHighlighting = _enableCodeHighlighting);

        this.WhenAnyValue(vm => vm.ShowCodeLineNumbers)
            .Where(showLineNumbers => showLineNumbers != _userStorage.Current.ShowCodeLineNumbers)
            .Subscribe(_ => _userStorage.Current.ShowCodeLineNumbers = _showCodeLineNumbers);

        this.WhenAnyValue(vm => vm.ExecutionSpeed)
            .Where(speed => speed != _userStorage.Current.LastSpeed)
            .Subscribe(_ => _userStorage.Current.LastSpeed = _executionSpeed);

        this.WhenAnyValue(vm => vm.SelectedLanguage)
            .Where(language => language != _userStorage.Current.LastLanguage)
            .Subscribe(_ => _userStorage.Current.LastLanguage = _selectedLanguage);
    }

    private void InitializeGameEngine()
    {
        this.WhenAnyValue(vm => vm.SelectedLanguage)
            .Skip(1)
            .Subscribe(language =>
            {
                if (!IsRunning)
                    SwitchEngineLanguage(language);
            });

        this.WhenAnyValue(vm => vm.ExecutionSpeed)
            .Skip(1)
            .Subscribe(speed =>
            {
                if (!IsRunning)
                    SetEngineSpeed(speed);
            });
    }

    private void SwitchEngineLanguage(Language language)
    {
        try
        {
            _robotGameEngine.Language = language;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to switch game engine language to {Language}", language);
        }
    }

    private void SetEngineSpeed(ExecutionSpeed speed)
    {
        try
        {
            _robotGameEngine.Speed = speed;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to set game engine speed to {Speed}", speed);
        }
    }

    public async Task InitializeGameEngineAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                _robotGameEngine.Language = SelectedLanguage;
                _robotGameEngine.Speed = ExecutionSpeed;
            });

            //var map = await GetGameMap.Handle(Unit.Default);
            var map = CurrentMap;

            if (map is not null)
                _robotGameEngine.CurrentMap = map;

            var (input, output) = await GetTerminalStreams.Handle(Unit.Default);
            _robotGameEngine.SetIo(output, input);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Game engine warm-up failed");
        }
    }

    #endregion

    #region Reactive commands

    [ReactiveCommand]
    private void CloseTheTerminal()
    {
        IsTerminalVisible = false;
        IsTerminalExists = false;
    }

    [ReactiveCommand]
    private async Task ExecuteCode()
    {
        await PrepareExecution();
        await RunCode();
        await FinalizeExecution();
    }

    [ReactiveCommand]
    private async Task RunSelectedCode()
    {
        var code = await GetSelectedCode.Handle(Unit.Default);
        if (string.IsNullOrWhiteSpace(code))
            return;

        await PrepareExecution();
        await RunCode(code);
        await FinalizeExecution();
    }

    [ReactiveCommand]
    private void ToggleShowCodeLineNumbers()
        => ShowCodeLineNumbers = !ShowCodeLineNumbers;

    [ReactiveCommand]
    private void ToggleEnableCodeHighlighting()
        => EnableCodeHighlighting = !EnableCodeHighlighting;

    [ReactiveCommand]
    private async Task IncrementEditorFontSize()
    {
        if (TextEditorFontSize >= _fontSizeOptions.Value.MaxFontSize)
            return;

        TextEditorFontSize += 1;
        await ShowEditorFontSizeToast();
    }

    [ReactiveCommand]
    private async Task DecrementEditorFontSize()
    {
        if (TextEditorFontSize <= _fontSizeOptions.Value.MinFontSize)
            return;

        TextEditorFontSize -= 1;
        await ShowEditorFontSizeToast();
    }

    [ReactiveCommand]
    private void ChangeTerminalVisibility()
    {
        IsTerminalExists = true;
        IsTerminalVisible = !IsTerminalVisible;
    }

    [ReactiveCommand]
    private async Task UpdateFontSize(int fontSize)
    {
        if (TextEditorFontSize == fontSize)
            return;

        TextEditorFontSize = fontSize;
        await ShowEditorFontSizeToast();
    }

    [ReactiveCommand]
    private async Task IncrementTerminalFontSize()
    {
        if (TerminalFontSize >= _fontSizeOptions.Value.MaxFontSize)
            return;

        TerminalFontSize += 1;
        await ShowTerminalFontSizeToast();
    }

    [ReactiveCommand]
    private async Task DecrementTerminalFontSize()
    {
        if (TerminalFontSize <= _fontSizeOptions.Value.MinFontSize)
            return;

        TerminalFontSize -= 1;
        await ShowTerminalFontSizeToast();
    }

    [ReactiveCommand]
    private async Task UpdateTerminalFontSize(int fontSize)
    {
        if (TerminalFontSize == fontSize)
            return;

        TerminalFontSize = fontSize;
        await ShowTerminalFontSizeToast();
    }

    [ReactiveCommand]
    private void ChangeExecutionSpeed(string? speed)
    {
        if (Enum.TryParse<ExecutionSpeed>(speed, out var value))
            ExecutionSpeed = value;
    }

    [ReactiveCommand]
    private void ChangeLanguage(Language language)
    {
        SelectedLanguage = language;
    }

    [ReactiveCommand]
    private void SelectMap(MapNumberItem item)
    {
        var index = item.Number - 1;
        if (index < 0 || index >= CurrentGame.Maps.Count)
            return;

        CurrentMap = CurrentGame.Maps[index];

        foreach (var mapNumber in MapNumbers)
            mapNumber.IsSelected = ReferenceEquals(mapNumber, item);
    }

    [ReactiveCommand]
    private void ToggleShowSidebar()
    {
        ShowSidebar = !ShowSidebar;
    }

    [ReactiveCommand]
    private void ToggleUseSystemTitleBar()
    {
        UseOsDecoration = !UseOsDecoration;
    }

    [ReactiveCommand]
    private void SelectCodeTheme(string themeName)
    {
        _userStorage.Current.CodeTheme = themeName;
        MessageBus.Current.SendMessage(new CodeThemeChangedMessage(themeName));
    }

    [ReactiveCommand]
    private async Task OpenAboutAsync()
        => await OpenAbout.Handle(Unit.Default);

    [ReactiveCommand]
    private async Task OpenAboutAvaloniaAsync()
        => await OpenAboutAvalonia.Handle(Unit.Default);

    [ReactiveCommand]
    private async Task OpenSettingsAsync()
    {
        var settingsViewModel = new SettingsViewModel(
            _userStorage,
            _themeSettingsStorage,
            _pythonSettingsStorage,
            _csharpSettingsStorage,
            _defaultUserOptions,
            _defaultThemeSettingsOptions,
            _pythonDefaultSettingsOptions,
            _csharpDefaultSettingsOptions,
            _codeExamplePathOptions,
            _fontSizeOptions,
            _loadingOptions,
            _themeCatalog,
            _themeCopier,
            _themeDeleter,
            _themeEditor,
            _themeCreator,
            _themeOptions,
            ZaggyAssets,
            _pythonFunctionNameValidator,
            _loggerFactory);
        await OpenSettings.Handle(settingsViewModel);
    }

    #endregion

    #region Private methods

    private async Task ShowEditorFontSizeToast()
        => await ShowToast.Handle($"Размер шрифта редактора изменён на {TextEditorFontSize}");

    private async Task ShowTerminalFontSizeToast()
        => await ShowToast.Handle($"Размер шрифта терминала изменён на {TerminalFontSize}");

    private async Task PrepareExecution()
    {
        lock (_executionLock)
        {
            if (_isRunning)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
            else
            {
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        await StopCodeExecution.Handle(Unit.Default);
    }

    private async Task RunCode(string? codeOverride = null)
    {
        try
        {
            _logger.LogDebug("Code execution was requested");
            var code = codeOverride ?? await GetCodeToExecute.Handle(Unit.Default);
            var (input, output) = await GetTerminalStreams.Handle(Unit.Default);
            //var map = await GetGameMap.Handle(Unit.Default);
            var map = CurrentMap;

#if DEBUG
            SelectedLanguage = Language.Python;
#endif
            _robotGameEngine.Language = SelectedLanguage;
            _robotGameEngine.Speed = ExecutionSpeed;
            _robotGameEngine.SetIo(output, input);

            if (map is not null)
                _robotGameEngine.CurrentMap = map;

            Debug.Assert(_cancellationTokenSource is not null);
            await _robotGameEngine.RunCodeAsync(code, _cancellationTokenSource.Token);

            await ConcludeRun.Handle(Unit.Default);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while running code");
        }
    }

    private async Task FinalizeExecution()
    {
        lock (_executionLock)
        {
            _isRunning = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        await StopCodeExecution.Handle(Unit.Default);
        await ResetMap.Handle(Unit.Default);
    }
#pragma warning disable AsyncVoidEventHandlerMethod
    private async void OnDebugLineUpdated(object? sender, DebugLineUpdatedEventArgs args)
#pragma warning restore AsyncVoidEventHandlerMethod
    {
        await UpdateCodeLine.Handle(args.LineNumber);
    }

    private void OnCodeErrorOccurred(object? sender, CodeErrorOccurredEventArgs args)
    {
        _logger.LogWarning("Code error: {Text}", args.Text);
    }

    #endregion
}
