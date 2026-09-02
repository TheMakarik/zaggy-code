namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class AppearanceSettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private int _codeFontSize;
    [Reactive] private int _terminalFontSize;
    [Reactive] private bool _useSystemTitleBar;
    [Reactive] private bool _showSidebar;
    [Reactive] private string _selectedCodeTheme = string.Empty;
    [Reactive] private string _selectedAppTheme = string.Empty;
    [Reactive] private bool _isLoadingThemes = true;
    [Reactive] private bool _enableCodeHighlighting;
    [Reactive] private bool _showCodeLineNumbers;
    [Reactive] private bool _hasChanges;
    [Reactive] private bool _canDecreaseCodeFontSize;
    [Reactive] private bool _canIncreaseCodeFontSize;
    [Reactive] private bool _canDecreaseTerminalFontSize;
    [Reactive] private bool _canIncreaseTerminalFontSize;

    #endregion

    private readonly FontSizeOptions _fontSizeOptions;
    private readonly UserData _defaultUserData;
    private readonly IObservableStorage<UserData> _userStorage;
    private readonly ILogger<AppearanceSettingsViewModel> _logger;
    private int _originalCodeFontSize;
    private int _originalTerminalFontSize;
    private bool _originalUseSystemTitleBar;
    private bool _originalShowSidebar;
    private string _originalCodeTheme = string.Empty;
    private string _originalSelectedAppTheme = string.Empty;
    private bool _originalEnableCodeHighlighting;
    private bool _originalShowCodeLineNumbers;

    public int MinFontSize => _fontSizeOptions.MinFontSize;
    public int MaxFontSize => _fontSizeOptions.MaxFontSize;
    public int LoadingDotsMaxCount { get; }

    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public ObservableCollection<ThemeMetadataItem> AvailableAppThemes { get; } = [];
    public Interaction<string, string> ApplyCodeThemeInteraction { get; } = new();
    public string CSharpExamplePath { get; }
    public string PythonExamplePath { get; }

    public AppearanceSettingsViewModel(
        IObservableStorage<UserData> userStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions,
        IOptions<LoadingOptions> loadingOptions,
        IThemeCatalog themeCatalog,
        ILogger<AppearanceSettingsViewModel> logger)
    {
        _fontSizeOptions = fontSizeOptions.Value;
        _defaultUserData = defaultUserOptions.Value.User;
        _userStorage = userStorage;
        _logger = logger;
        LoadingDotsMaxCount = loadingOptions.Value.LoadingDotsMaxCount;
        CSharpExamplePath = codeExamplePathOptions.Value.CSharpExamplePath;
        PythonExamplePath = codeExamplePathOptions.Value.PythonExamplePath;

        var current = userStorage.Current;
        _originalCodeFontSize = current.CodeFontSize;
        _originalTerminalFontSize = current.TerminalFontSize;
        _originalUseSystemTitleBar = current.UseSystemTitleBar;
        _originalShowSidebar = current.ShowSidebar;
        _originalCodeTheme = current.CodeTheme;
        _originalSelectedAppTheme = current.CurrentTheme;
        _originalEnableCodeHighlighting = current.EnableCodeHighlighting;
        _originalShowCodeLineNumbers = current.ShowCodeLineNumbers;

#pragma warning disable AsyncVoidMethod
        _ = InitializeAvailableAppThemesAsync(themeCatalog);
#pragma warning restore AsyncVoidMethod

        LoadFromUserData(current);
        HasChanges = false;

        InitializeMessageBusSubscriptions();

        this.WhenAnyPropertyChanged(
                nameof(CodeFontSize),
                nameof(TerminalFontSize),
                nameof(UseSystemTitleBar),
                nameof(ShowSidebar),
                nameof(SelectedCodeTheme),
                nameof(SelectedAppTheme),
                nameof(EnableCodeHighlighting),
                nameof(ShowCodeLineNumbers))
            .Subscribe(_ => UpdateHasChanges());

        this.WhenAnyValue(viewModel => viewModel.SelectedCodeTheme)
            .Skip(1)
            .SelectMany(themeName => ApplyCodeThemeInteraction.Handle(themeName))
            .Subscribe();

        this.WhenAnyValue(viewModel => viewModel.CodeFontSize)
            .Subscribe(_ =>
            {
                CanDecreaseCodeFontSize = CodeFontSize > MinFontSize;
                CanIncreaseCodeFontSize = CodeFontSize < MaxFontSize;
            });

        this.WhenAnyValue(viewModel => viewModel.TerminalFontSize)
            .Subscribe(_ =>
            {
                CanDecreaseTerminalFontSize = TerminalFontSize > MinFontSize;
                CanIncreaseTerminalFontSize = TerminalFontSize < MaxFontSize;
            });
    }

    public void ResetToDefaults()
    {
        CodeFontSize = _defaultUserData.CodeFontSize;
        TerminalFontSize = _defaultUserData.TerminalFontSize;
        UseSystemTitleBar = _defaultUserData.UseSystemTitleBar;
        ShowSidebar = _defaultUserData.ShowSidebar;
        SelectedCodeTheme = _defaultUserData.CodeTheme;
        EnableCodeHighlighting = _defaultUserData.EnableCodeHighlighting;
        ShowCodeLineNumbers = _defaultUserData.ShowCodeLineNumbers;

        ApplyAppThemeSelection(_defaultUserData.CurrentTheme);
    }

    public void AcceptChanges()
    {
        _originalCodeFontSize = CodeFontSize;
        _originalTerminalFontSize = TerminalFontSize;
        _originalUseSystemTitleBar = UseSystemTitleBar;
        _originalShowSidebar = ShowSidebar;
        _originalCodeTheme = SelectedCodeTheme;
        _originalSelectedAppTheme = SelectedAppTheme;
        _originalEnableCodeHighlighting = EnableCodeHighlighting;
        _originalShowCodeLineNumbers = ShowCodeLineNumbers;
        HasChanges = false;
    }

    private void LoadFromUserData(UserData userData)
    {
        CodeFontSize = userData.CodeFontSize;
        TerminalFontSize = userData.TerminalFontSize;
        UseSystemTitleBar = userData.UseSystemTitleBar;
        ShowSidebar = userData.ShowSidebar;
        SelectedCodeTheme = userData.CodeTheme;
        EnableCodeHighlighting = userData.EnableCodeHighlighting;
        ShowCodeLineNumbers = userData.ShowCodeLineNumbers;
    }

    private void UpdateHasChanges()
    {
        HasChanges =
            CodeFontSize != _originalCodeFontSize ||
            TerminalFontSize != _originalTerminalFontSize ||
            UseSystemTitleBar != _originalUseSystemTitleBar ||
            ShowSidebar != _originalShowSidebar ||
            SelectedCodeTheme != _originalCodeTheme ||
            SelectedAppTheme != _originalSelectedAppTheme ||
            EnableCodeHighlighting != _originalEnableCodeHighlighting ||
            ShowCodeLineNumbers != _originalShowCodeLineNumbers;
    }

    private void InitializeMessageBusSubscriptions()
    {
        MessageBus.Current.Listen<CodeThemesResponseMessage>()
            .Subscribe(message =>
            {
                AvailableCodeThemes.Clear();
                foreach (var theme in message.Themes)
                    AvailableCodeThemes.Add(theme);
            });

        MessageBus.Current.SendMessage(new CodeThemesRequestMessage());
    }

    [ReactiveCommand]
    private void SelectCodeTheme(string themeName)
    {
        SelectedCodeTheme = themeName;
    }

    [ReactiveCommand]
    private void SelectAppTheme(string themeName)
        => ApplyAppThemeSelection(themeName);

    private void ApplyAppThemeSelection(string themeName)
    {
        if (SelectedAppTheme == themeName)
            return;

        SelectedAppTheme = themeName;
        MessageBus.Current.SendMessage(new AppThemeChangedMessage(themeName));
    }

    public void RevertChanges()
    {
        MessageBus.Current.SendMessage(new AppThemeChangedMessage(_originalSelectedAppTheme));
        SelectedAppTheme = _originalSelectedAppTheme;
    }

    private async Task InitializeAvailableAppThemesAsync(IThemeCatalog themeCatalog)
    {
        try
        {
            // Чтение архивов — синхронный файловый IO внутри async-обёртки,
            // поэтому уводим в пул потоков, чтобы окно настроек не подвисало.
            var themes = await Task.Run(async () => await themeCatalog.GetAvailableThemesAsync());

            foreach (var metadata in themes)
                AvailableAppThemes.Add(ThemeMetadataItem.From(metadata));

            var saved = _userStorage.Current.CurrentTheme;
            SelectedAppTheme = AvailableAppThemes.Any(theme => theme.Name == saved) ? saved : string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load application themes");
        }
        finally
        {
            IsLoadingThemes = false;
        }
    }

    [ReactiveCommand]
    private void ChangeCodeFontSize(string? deltaText)
    {
        if (!int.TryParse(deltaText, out var delta))
        {
            _logger.LogWarning("Failed to parse code font size delta: {DeltaText}", deltaText);
            return;
        }

        CodeFontSize = Math.Clamp(CodeFontSize + delta, _fontSizeOptions.MinFontSize, _fontSizeOptions.MaxFontSize);
    }

    [ReactiveCommand]
    private void ChangeTerminalFontSize(string? deltaText)
    {
        if (!int.TryParse(deltaText, out var delta))
        {
            _logger.LogWarning("Failed to parse terminal font size delta: {DeltaText}", deltaText);
            return;
        }

        TerminalFontSize = Math.Clamp(TerminalFontSize + delta, _fontSizeOptions.MinFontSize, _fontSizeOptions.MaxFontSize);
    }
}
