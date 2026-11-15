using ZaggyCode.Core.LanguageRuntime.Enums;
using ZaggyCode.Core.GamesCore.Enums.Metadata;

namespace ZaggyCode.Avalonia.ViewModels.Main;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _isTerminalVisible = true;
    [Reactive] private bool _isTerminalExists = true;
    [Reactive] private bool _isRunning = false;
    [Reactive] private bool _useOsDecoration = false;
    [Reactive] private SidebarPosition _sidebarPosition = SidebarPosition.Left;
    [Reactive] private ExecutionSpeed _executionSpeed;
    [Reactive] private Language _selectedLanguage;
    [Reactive] private LanguageItem? _selectedLanguageItem;
    [Reactive] private RobotGame _currentRobotGame;
    [Reactive] private RobotMap _currentRobotMap;
    [Reactive] private int _textEditorFontSize;
    [Reactive] private int _terminalFontSize;
    [Reactive] private bool _isDescriptionVisible;

    #endregion

    #region Properties

    public int MaxFontSize { get; init; }
    public int MinFontSize { get; init; }
    public bool IsSidebarLeft => SidebarPosition == SidebarPosition.Left;
    public bool IsSidebarRight => SidebarPosition == SidebarPosition.Right;
    public bool IsSidebarTop => SidebarPosition == SidebarPosition.Top;
    public bool IsVerticalSidebarVisible => IsSidebarLeft || IsSidebarRight;
    public bool IsHorizontalSidebarVisible => IsSidebarTop;
    public int SidebarColumn => IsSidebarRight ? 2 : 0;
    public int SidebarRow => IsSidebarTop ? 2 : 4;
    public string CodeTheme { get; private set; }
    public PythonSettings PythonSettings => _pythonSettingsStorage.Value;
    public CSharpSettings CSharpSettings => _csharpSettingsStorage.Value;
    public PopupOptions PopupOptions { get; }
    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public ObservableCollection<LanguageItem> AvailableLanguages { get; } = [];
    public ObservableCollection<MapNumberItem> MapNumbers { get; } = [];
    public IOptions<ZaggyAssetsOptions> ZaggyAssets { get; init; }

    public bool IsCodeVisible => !IsDescriptionVisible;
    public string DescriptionToggleToolTip => IsDescriptionVisible
        ? "Показать код"
        : "Показать описание уровня";

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
    // Шаг анимации не должен быть длиннее паузы между строками, иначе
    // следующий ход обрывает предыдущий и спрайт прыгает.
    public double MoveStepSeconds => Math.Min(
        _animationOptions.Value.MoveStepSeconds,
        _executionSpeed.GetActual(_speedOptions.Value) / 1000.0);

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
    public readonly Interaction<Unit, Unit> StopCodeExecution = new();
    public readonly Interaction<Unit, Unit> ResetMap = new();
    public readonly Interaction<Unit, Unit> ConcludeRun = new();
    public readonly Interaction<ToastRequest, Unit> ShowToast = new();
    public readonly Interaction<SettingsViewModel, Unit> OpenSettings = new();
    public readonly Interaction<Unit, Unit> OpenAbout = new();

    #endregion

    #region Services

    private readonly IRobotGameEngineFactory _robotGameEngineFactory;
    private readonly IRobotGameTerminalProxy _robotGameTerminalProxy;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISettingsStorage<StateSettings> _stateSettingsStorage;
    private readonly ISettingsStorage<ViewSettings> _viewSettingsStorage;
    private readonly ISettingsStorage<PythonSettings> _pythonSettingsStorage;
    private readonly ISettingsStorage<CSharpSettings> _csharpSettingsStorage;
    private readonly IOptions<FontSizeOptions> _fontSizeOptions;
    private readonly IOptions<CodeThemeDisplayNameOptions> _displayNameOptions;
    private readonly IOptions<CodeThemeIconOptions> _iconOptions;
    private readonly IOptions<MapAssetsOptions> _mapAssetsOptions;
    private readonly IOptions<MapAnimationOptions> _animationOptions;
    private readonly IOptions<MapOptions> _mapOptions;
    private readonly IOptions<SpeedMillisecondsOptions> _speedOptions;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly object _executionLock = new();
    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Constructors

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IRobotGameEngineFactory robotGameEngineFactory,
        IRobotGameTerminalProxy robotGameTerminalProxy,
        IServiceProvider serviceProvider,
        IOptions<ZaggyAssetsOptions> zaggyAssets,
        ISettingsStorage<StateSettings> stateSettingsStorage,
        ISettingsStorage<ViewSettings> viewSettingsStorage,
        ISettingsStorage<PythonSettings> pythonSettingsStorage,
        ISettingsStorage<CSharpSettings> csharpSettingsStorage,
        IOptions<PopupOptions> popupOptions,
        IOptions<FontSizeOptions> textFontSize,
        IOptions<CodeThemeDisplayNameOptions> displayNameOptions,
        IOptions<CodeThemeIconOptions> iconOptions,
        IOptions<MapAssetsOptions> mapAssetsOptions,
        IOptions<MapAnimationOptions> animationOptions,
        IOptions<MapOptions> mapOptions,
        IOptions<SpeedMillisecondsOptions> speedOptions)
    {
        _robotGameEngineFactory = robotGameEngineFactory;
        _robotGameTerminalProxy = robotGameTerminalProxy;
        _serviceProvider = serviceProvider;
        ZaggyAssets = zaggyAssets;
        _logger = logger;
        _fontSizeOptions = textFontSize;
        _stateSettingsStorage = stateSettingsStorage;
        _viewSettingsStorage = viewSettingsStorage;
        _pythonSettingsStorage = pythonSettingsStorage;
        _csharpSettingsStorage = csharpSettingsStorage;
        _displayNameOptions = displayNameOptions;
        _iconOptions = iconOptions;
        _mapAssetsOptions = mapAssetsOptions;
        _animationOptions = animationOptions;
        _mapOptions = mapOptions;
        _speedOptions = speedOptions;
        _executionSpeed = stateSettingsStorage.Value.LastSpeed;
        _selectedLanguage = stateSettingsStorage.Value.LastLanguage;
        _textEditorFontSize = viewSettingsStorage.Value.CodeFontSize;
        _terminalFontSize = viewSettingsStorage.Value.TerminalFontSize;
        _useOsDecoration = viewSettingsStorage.Value.UseSystemTitleBar;
        _sidebarPosition = viewSettingsStorage.Value.SidebarPosition;
        CodeTheme = viewSettingsStorage.Value.CodeTheme;
        PopupOptions = popupOptions.Value;
        MaxFontSize = _fontSizeOptions.Value.MaxFontSize;
        MinFontSize = _fontSizeOptions.Value.MinFontSize;
        CurrentRobotGame = CreateTestGame();
        Debug.Assert(CurrentRobotGame.Maps.Count <= _mapOptions.Value.MaxMaps, "Слишком много карт");

        for (var index = 0; index < CurrentRobotGame.Maps.Count; index++)
            MapNumbers.Add(new MapNumberItem(index + 1));

        CurrentRobotMap = CurrentRobotGame.Maps[0];
        MapNumbers[0].IsSelected = true;

        InitializeMessageBusSubscriptions();
        InitializeAvailableCodeThemes();
        InitializeAvailableLanguages();
        InitializeLanguageSelection();
        InitializeStorageSynchronization();
        ObserveSettingsStorages();

        this.WhenAnyValue(vm => vm.IsDescriptionVisible)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsCodeVisible));
                this.RaisePropertyChanged(nameof(DescriptionToggleToolTip));
            });

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

    // Тестовая игра: три карты, require-draw образует букву Г разных размеров.
    public static RobotGame CreateTestGame()
    {
        ObservableCollection<RobotMap> maps = [.. TestMapSizes.Select(CreateLetterMap)];

        return new RobotGame
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

    private static RobotMap CreateLetterMap(int size)
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
            points.First(point => point.X == 1 && point.Y == y).WallType = RobotWallType.Full;

        for (var x = 2; x < size; x++)
            points.First(point => point.X == x && point.Y == 1).WallType = RobotWallType.Full;

        // Монетки в скрытой комнате за стеной (закраска там не требуется).
        points.First(point => point.X == size - 1 && point.Y == size - 1).HasCoin = true;
        if (size > 3)
            points.First(point => point.X == 2 && point.Y == 2).HasCoin = true;

        // На большой карте — боковая стеночка между монетками.
        if (size > 4)
            points.First(point => point.X == 3 && point.Y == 3).WallType = RobotWallType.BottomRight;

        // Немного скруглить внешние углы карты.
        points.First(point => point.X == 0 && point.Y == 0).CustomBorderRadiusLeftTop = cornerRadius;
        points.First(point => point.X == size - 1 && point.Y == 0).CustomBorderRadiusRightTop = cornerRadius;
        points.First(point => point.X == 0 && point.Y == size - 1).CustomBorderRadiusLeftBottom = cornerRadius;
        points.First(point => point.X == size - 1 && point.Y == size - 1).CustomBorderRadiusRightBottom = cornerRadius;

        return new RobotMap
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
            var iconName = (string?)iconProperty?.GetValue(_iconOptions.Value) ?? string.Empty;

            AvailableCodeThemes.Add(new CodeThemeItem(themeName, displayName, iconName));
        }
    }

    private void InitializeAvailableLanguages()
    {
        foreach (var language in Enum.GetValues<Language>())
            AvailableLanguages.Add(new LanguageItem(language, language.GetPrettyName()));
    }

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

#pragma warning disable AsyncVoidMethod
        MessageBus.Current.Listen<SettingsSavedMessage>()
            .Subscribe(async void (_) => await ShowToast.Handle(new ToastRequest("Настройки сохранены")));

        MessageBus.Current.Listen<SettingsResetMessage>()
            .Subscribe(async void (_) => await ShowToast.Handle(new ToastRequest("Настройки сброшены")));
#pragma warning restore AsyncVoidMethod
    }

    private void InitializeStorageSynchronization()
    {
        this.WhenAnyValue(vm => vm.TextEditorFontSize)
            .Where(size => size != _viewSettingsStorage.Value.CodeFontSize)
            .Subscribe(_ =>
            {
                _viewSettingsStorage.Value.CodeFontSize = _textEditorFontSize;
                _viewSettingsStorage.FlashAtBackground(true);
            });

        this.WhenAnyValue(vm => vm.TerminalFontSize)
            .Where(size => size != _viewSettingsStorage.Value.TerminalFontSize)
            .Subscribe(_ =>
            {
                _viewSettingsStorage.Value.TerminalFontSize = _terminalFontSize;
                _viewSettingsStorage.FlashAtBackground(true);
            });

        this.WhenAnyValue(vm => vm.UseOsDecoration)
            .Where(useOsDecoration => useOsDecoration != _viewSettingsStorage.Value.UseSystemTitleBar)
            .Subscribe(_ => _viewSettingsStorage.Value.UseSystemTitleBar = _useOsDecoration);

        this.WhenAnyValue(vm => vm.SidebarPosition)
            .Subscribe(position =>
            {
                if (position != _viewSettingsStorage.Value.SidebarPosition)
                    _viewSettingsStorage.Value.SidebarPosition = position;

                RaiseSidebarLayoutChanged();
            });

        this.WhenAnyValue(vm => vm.ExecutionSpeed)
            .Where(speed => speed != _stateSettingsStorage.Value.LastSpeed)
            .Subscribe(_ => _stateSettingsStorage.Value.LastSpeed = _executionSpeed);

        this.WhenAnyValue(vm => vm.SelectedLanguage)
            .Where(language => language != _stateSettingsStorage.Value.LastLanguage)
            .Subscribe(_ => _stateSettingsStorage.Value.LastLanguage = _selectedLanguage);
    }

    private void ObserveSettingsStorages()
    {
        _viewSettingsStorage.Value.PropertyChanged += (_, args) => ApplyViewSettingsChange(args.PropertyName);
        _stateSettingsStorage.Value.PropertyChanged += (_, args) => ApplyStateSettingsChange(args.PropertyName);
    }

    private void ApplyViewSettingsChange(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(ViewSettings.CodeFontSize):
                TextEditorFontSize = _viewSettingsStorage.Value.CodeFontSize;
                ShowEditorFontSizeToast();
                break;
            case nameof(ViewSettings.TerminalFontSize):
                TerminalFontSize = _viewSettingsStorage.Value.TerminalFontSize;
                ShowTerminalFontSizeToast();
                break;
            case nameof(ViewSettings.UseSystemTitleBar):
                UseOsDecoration = _viewSettingsStorage.Value.UseSystemTitleBar;
                break;
            case nameof(ViewSettings.SidebarPosition):
                SidebarPosition = _viewSettingsStorage.Value.SidebarPosition;
                break;
            case nameof(ViewSettings.CodeTheme):
                CodeTheme = _viewSettingsStorage.Value.CodeTheme;
                break;
        }
    }

    private void ApplyStateSettingsChange(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(StateSettings.LastSpeed):
                ExecutionSpeed = _stateSettingsStorage.Value.LastSpeed;
                break;
            case nameof(StateSettings.LastLanguage):
                SelectedLanguage = _stateSettingsStorage.Value.LastLanguage;
                break;
        }
    }

    private void RaiseSidebarLayoutChanged()
    {
        this.RaisePropertyChanged(nameof(IsSidebarLeft));
        this.RaisePropertyChanged(nameof(IsSidebarRight));
        this.RaisePropertyChanged(nameof(IsSidebarTop));
        this.RaisePropertyChanged(nameof(IsVerticalSidebarVisible));
        this.RaisePropertyChanged(nameof(IsHorizontalSidebarVisible));
        this.RaisePropertyChanged(nameof(SidebarColumn));
        this.RaisePropertyChanged(nameof(SidebarRow));
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
        await RunCode(code, ExecutionType.AnalyzeOnly);
        await FinalizeExecution();
    }

    [ReactiveCommand]
    private void IncrementEditorFontSize()
    {
        if (TextEditorFontSize >= _fontSizeOptions.Value.MaxFontSize)
            return;

        TextEditorFontSize += 1;
    }

    [ReactiveCommand]
    private void DecrementEditorFontSize()
    {
        if (TextEditorFontSize <= _fontSizeOptions.Value.MinFontSize)
            return;

        TextEditorFontSize -= 1;
    }

    [ReactiveCommand]
    private void ChangeTerminalVisibility()
    {
        IsTerminalExists = true;
        IsTerminalVisible = !IsTerminalVisible;
    }

    [ReactiveCommand]
    private void UpdateFontSize(int fontSize)
    {
        if (TextEditorFontSize == fontSize)
            return;

        TextEditorFontSize = fontSize;
    }

    [ReactiveCommand]
    private void IncrementTerminalFontSize()
    {
        if (TerminalFontSize >= _fontSizeOptions.Value.MaxFontSize)
            return;

        TerminalFontSize += 1;
    }

    [ReactiveCommand]
    private void DecrementTerminalFontSize()
    {
        if (TerminalFontSize <= _fontSizeOptions.Value.MinFontSize)
            return;

        TerminalFontSize -= 1;
    }

    [ReactiveCommand]
    private void UpdateTerminalFontSize(int fontSize)
    {
        if (TerminalFontSize == fontSize)
            return;

        TerminalFontSize = fontSize;
    }

    [ReactiveCommand]
    private void ChangeExecutionSpeed(string? speed)
    {
        if (!Enum.TryParse<ExecutionSpeed>(speed, out var value))
            return;

        ExecutionSpeed = value;
        this.RaisePropertyChanged(nameof(MoveStepSeconds));
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
        if (index < 0 || index >= CurrentRobotGame.Maps.Count)
            return;

        CurrentRobotMap = CurrentRobotGame.Maps[index];

        foreach (var mapNumber in MapNumbers)
            mapNumber.IsSelected = ReferenceEquals(mapNumber, item);
    }

    [ReactiveCommand]
    private void ToggleDescription()
        => IsDescriptionVisible = !IsDescriptionVisible;

    [ReactiveCommand]
    private void ChangeSidebarPosition(string? position)
    {
        if (Enum.TryParse<SidebarPosition>(position, out var value))
            SidebarPosition = value;
    }

    [ReactiveCommand]
    private void ToggleUseSystemTitleBar()
    {
        UseOsDecoration = !UseOsDecoration;
    }

    [ReactiveCommand]
    private void SelectCodeTheme(string themeName)
        => _viewSettingsStorage.Value.CodeTheme = themeName;

    [ReactiveCommand]
    private async Task OpenAboutAsync()
        => await OpenAbout.Handle(Unit.Default);

    [ReactiveCommand]
    private async Task OpenSettingsAsync()
    {
        // Перед открытием настроек сбрасываем накопленные в памяти изменения в файлы,
        // чтобы окно настроек и отмена работали от сохранённого состояния.
        await _serviceProvider.GetRequiredService<ISettingStorageFacade>().FlushAllAsync();

        var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
        await OpenSettings.Handle(settingsViewModel);
    }

    #endregion

    #region Private methods

    public void RevertViewSettings()
        => _viewSettingsStorage.Revert();

    public void FlushViewSettings()
        => _viewSettingsStorage.FlashAtBackground();

    private void ShowEditorFontSizeToast()
        => _ = ShowToast.Handle(new ToastRequest($"Размер шрифта редактора и описания изменён на {TextEditorFontSize}", true));

    private void ShowTerminalFontSizeToast()
        => _ = ShowToast.Handle(new ToastRequest($"Размер шрифта терминала изменён на {TerminalFontSize}", true));

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

    private async Task RunCode(string? codeOverride = null, ExecutionType executionType = ExecutionType.ExecuteWithProxies)
    {
        try
        {
            _logger.LogDebug("Запрошено выполнение кода");
            var code = codeOverride ?? await GetCodeToExecute.Handle(Unit.Default);
            var (input, output) = await GetTerminalStreams.Handle(Unit.Default);

            Debug.Assert(CurrentRobotMap is not null);
            Debug.Assert(_cancellationTokenSource is not null);

            var engineDto = new RobotGameEngineInitDto
            {
                Input = input,
                Output = output,
                Language = SelectedLanguage,
                Speed = ExecutionSpeed,
                Map = CurrentRobotMap
            };

            var cancellationToken = _cancellationTokenSource.Token;
            var engine = await _robotGameEngineFactory.CreateAsync(engineDto, cancellationToken);
            var result = await engine.ExecuteAsync(code, cancellationToken, executionType);

            if (result.Error is { } error && !string.IsNullOrWhiteSpace(error.ErrorMessage))
                _robotGameTerminalProxy.WriteError(error.ErrorMessage);

            await ConcludeRun.Handle(Unit.Default);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Выполнение кода отменено");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка при выполнении кода");
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

    #endregion
}
