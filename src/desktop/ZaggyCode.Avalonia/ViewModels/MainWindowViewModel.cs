namespace ZaggyCode.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _isTerminalVisible = true;
    [Reactive] private bool _isTerminalExists = true;
    [Reactive] private bool _isRunning = false;
    [Reactive] private bool _useOsDecoration = false;
    [Reactive] private bool _showSidebar = true;
    [Reactive] private ExecutionSpeed _executionSpeed;
    [Reactive] private Language _selectedLanguage;
    [Reactive] private int _textEditorFontSize;
    [Reactive] private int _terminalFontSize;

    #endregion

    #region Properties

    public int MaxFontSize { get; init; }
    public int MinFontSize { get; init; }
    public string CodeTheme { get; private set; }
    public PopupOptions PopupOptions { get; }

    public IRobotExecutor? Executor { get; set; }
    public TextReader? TerminalReader { get; set; }
    public TextWriter? TerminalWriter { get; set; }

    #endregion

    #region Interaction

    public readonly Interaction<Unit, Unit> ResizeGridToMax = new();
    public readonly Interaction<Unit, Unit> ClearTerminalContent = new();
    public readonly Interaction<Unit, Unit> BackGridToNormal = new();
    public readonly Interaction<Unit, string> GetCodeToExecute = new();
    public readonly Interaction<int, Unit> UpdateCodeLine = new();
    public readonly Interaction<Unit, Unit> StopCodeExecution = new();
    public readonly Interaction<Unit, Unit> ResetMap = new();
    public readonly Interaction<Unit, Unit> ConcludeRun = new();
    public readonly Interaction<SettingsViewModel, Unit> OpenSettings = new();

    #endregion

    #region Services

    private readonly IServiceScopeFactory _factory;
    private readonly IUserStorage _userStorage;
    private readonly IOptions<DefaultUser> _defaultUserOptions;
    private readonly IOptions<CodeExamplePathOptions> _codeExamplePathOptions;
    private readonly IOptions<FontSizeOptions> _fontSizeOptions;
    private readonly IOptions<CodeThemeDisplayNameOptions> _displayNameOptions;
    private readonly IOptions<CodeThemeIconOptions> _iconOptions;
    private readonly ILogger<MainWindowViewModel> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Constructors

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IServiceScopeFactory factory,
        IUserStorage userStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<PopupOptions> popupOptions,
        IOptions<FontSizeOptions> textFontSize,
        IOptions<CodeThemeDisplayNameOptions> displayNameOptions,
        IOptions<CodeThemeIconOptions> iconOptions)
    {
        _factory = factory;
        _userStorage = userStorage;
        _defaultUserOptions = defaultUserOptions;
        _codeExamplePathOptions = codeExamplePathOptions;
        _displayNameOptions = displayNameOptions;
        _iconOptions = iconOptions;
        _executionSpeed = userStorage.Current.LastSpeed;
        _selectedLanguage = userStorage.Current.LastLanguage;
        _textEditorFontSize = userStorage.Current.CodeFontSize;
        _terminalFontSize = userStorage.Current.TerminalFontSize;
        _useOsDecoration = userStorage.Current.UseSystemTitleBar;
        _showSidebar = userStorage.Current.ShowSidebar;
        CodeTheme = userStorage.Current.CodeTheme;
        PopupOptions = popupOptions.Value;
        _fontSizeOptions = textFontSize;
        MaxFontSize = _fontSizeOptions.Value.MaxFontSize;
        MinFontSize = _fontSizeOptions.Value.MinFontSize;
        _logger = logger;

        InitializeMessageBusSubscriptions();

        this.WhenAnyPropertyChanged().Subscribe(context =>
        {
#pragma warning disable AsyncVoidMethod
            this.WhenAnyValue(vm => vm.IsTerminalVisible)
                .Where(isVisible => !isVisible)
                .Subscribe(async void (onNext) => await ResizeGridToMax.Handle(Unit.Default));

            this.WhenAnyValue(vm => vm.IsTerminalVisible)
                .Where(isVisible => isVisible)
                .Subscribe(async void (onNext) => await BackGridToNormal.Handle(Unit.Default));

            this.WhenAnyValue(vm => vm.IsTerminalExists)
                .Where(isVisible => !isVisible)
                .Subscribe(async void (onNext) => await ClearTerminalContent.Handle(Unit.Default));

            this.WhenAnyValue(vm => vm.TextEditorFontSize)
                .Where(size => size != _userStorage.Current.CodeFontSize)
                .Subscribe(onNext => userStorage.Current.CodeFontSize = _textEditorFontSize);

            this.WhenAnyValue(vm => vm.TerminalFontSize)
                .Where(size => size != _userStorage.Current.TerminalFontSize)
                .Subscribe(onNext => userStorage.Current.TerminalFontSize = _terminalFontSize);

            this.WhenAnyValue(vm => vm.UseOsDecoration)
                .Where(useOsDecoration => useOsDecoration != _userStorage.Current.UseSystemTitleBar)
                .Subscribe(onNext => userStorage.Current.UseSystemTitleBar = _useOsDecoration);

            this.WhenAnyValue(vm => vm.ShowSidebar)
                .Where(showSidebar => showSidebar != _userStorage.Current.ShowSidebar)
                .Subscribe(onNext => userStorage.Current.ShowSidebar = _showSidebar);

            this.WhenAnyValue(vm => vm.ExecutionSpeed)
                .Where(speed => speed != _userStorage.Current.LastSpeed)
                .Subscribe(onNext => userStorage.Current.LastSpeed = _executionSpeed);

            this.WhenAnyValue(vm => vm.SelectedLanguage)
                .Where(language => language != _userStorage.Current.LastLanguage)
                .Subscribe(onNext => userStorage.Current.LastLanguage = _selectedLanguage);
#pragma warning restore AsyncVoidMethod
        });
    }

    private void InitializeMessageBusSubscriptions()
    {
        MessageBus.Current.Listen<CodeFontSizeChangedMessage>()
            .Subscribe(message => TextEditorFontSize = message.FontSize);

        MessageBus.Current.Listen<TerminalFontSizeChangedMessage>()
            .Subscribe(message => TerminalFontSize = message.FontSize);

        MessageBus.Current.Listen<UseSystemTitleBarChangedMessage>()
            .Subscribe(message => UseOsDecoration = message.UseSystemTitleBar);

        MessageBus.Current.Listen<ShowSidebarChangedMessage>()
            .Subscribe(message => ShowSidebar = message.ShowSidebar);

        MessageBus.Current.Listen<CodeThemeChangedMessage>()
            .Subscribe(message =>
            {
                _userStorage.Current.CodeTheme = message.ThemeName;
                CodeTheme = message.ThemeName;
            });
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
    private void ExecuteCode()
    {
        _ = Task.Factory.StartNew(async () =>
        {
            await PrepareExecution();
            await RunCode();
            await FinalizeExecution();
        }, TaskCreationOptions.LongRunning);
    }

    [ReactiveCommand]
    private void IncrementEditorFontSize()
    {
        if (TextEditorFontSize < _fontSizeOptions.Value.MaxFontSize)
            TextEditorFontSize += 1;
    }

    [ReactiveCommand]
    private void DecrementEditorFontSize()
    {
        if (TextEditorFontSize > _fontSizeOptions.Value.MinFontSize)
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
        TextEditorFontSize = fontSize;
        MessageBus.Current.SendMessage(new FontSizeToastMessage("редактора", fontSize));
    }

    [ReactiveCommand]
    private void IncrementTerminalFontSize()
    {
        if (TerminalFontSize < _fontSizeOptions.Value.MaxFontSize)
            TerminalFontSize += 1;
    }

    [ReactiveCommand]
    private void DecrementTerminalFontSize()
    {
        if (TerminalFontSize > _fontSizeOptions.Value.MinFontSize)
            TerminalFontSize -= 1;
    }

    [ReactiveCommand]
    private void UpdateTerminalFontSize(int fontSize)
    {
        TerminalFontSize = fontSize;
        MessageBus.Current.SendMessage(new FontSizeToastMessage("терминала", fontSize));
    }

    [ReactiveCommand]
    private void ChangeExecutionSpeed(ExecutionSpeed speed)
    {
        ExecutionSpeed = speed;
    }

    [ReactiveCommand]
    private void ChangeLanguage(Language language)
    {
        SelectedLanguage = language;
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
    private async Task OpenSettingsAsync()
    {
        var settingsViewModel = new SettingsViewModel(_userStorage, _defaultUserOptions, _codeExamplePathOptions, _fontSizeOptions, _displayNameOptions, _iconOptions);
        await OpenSettings.Handle(settingsViewModel);
    }

    #endregion

    #region Private methods

    private async Task PrepareExecution()
    {
        lock (this)
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

    private async Task RunCode()
    {
        try
        {
            _logger.LogDebug("Code execution was requested");
            var code = await GetCodeToExecute.Handle(Unit.Default);

#if DEBUG
            SelectedLanguage = Language.CSharp;
#endif
            await using var scope = _factory.CreateAsyncScope();
            var runner = scope.ServiceProvider.GetRequiredKeyedService<ILanguageRunner>(SelectedLanguage.GetLanguageExtension());

            Debug.Assert(TerminalReader is not null);
            Debug.Assert(TerminalWriter is not null);
            Debug.Assert(Executor is not null);

            runner.DebugLineUpdated += OnDebugLineUpdated;
            runner.CodeErrorOccurred += OnCodeErrorOccurred;

            Debug.Assert(_cancellationTokenSource is not null);

            runner
                .RedirectIo(TerminalReader, TerminalWriter)
                .SetExecutor(Executor)
                .SetSpeed(ExecutionSpeed)
                .Execute(code, _cancellationTokenSource.Token);

            await ConcludeRun.Handle(Unit.Default);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Code execution was cancelled");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while running code");
        }
    }

    private async Task FinalizeExecution()
    {
        lock (this)
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
        _logger.LogError("Code error: {Text}", args.Text);
    }

    #endregion
}