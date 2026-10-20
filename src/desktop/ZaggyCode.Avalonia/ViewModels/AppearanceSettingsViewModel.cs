namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class AppearanceSettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private int _codeFontSize;
    [Reactive] private int _terminalFontSize;
    [Reactive] private bool _useSystemTitleBar;
    [Reactive] private bool _showSidebar;
    [Reactive] private SidebarPosition _sidebarPosition = SidebarPosition.Left;
    [Reactive] private string _selectedCodeTheme = string.Empty;
    [Reactive] private string _selectedAppTheme = string.Empty;
    [Reactive] private ThemeCreationBehavior _themeCreationBehavior;
    [Reactive] private bool _isLoadingThemes = true;
    [Reactive] private bool _hasChanges;
    [Reactive] private bool _canDecreaseCodeFontSize;
    [Reactive] private bool _canIncreaseCodeFontSize;
    [Reactive] private bool _canDecreaseTerminalFontSize;
    [Reactive] private bool _canIncreaseTerminalFontSize;

    #endregion

    private readonly FontSizeOptions _fontSizeOptions;
    private readonly UserData _defaultUserData;
    private readonly IObservableStorage<UserData> _userStorage;
    private readonly IObservableStorage<ThemeSettings> _themeSettingsStorage;
    private readonly ThemeSettings _defaultThemeSettings;
    private readonly IThemeCatalog _themeCatalog;
    private readonly IThemeCopier _themeCopier;
    private readonly IThemeDeleter _themeDeleter;
    private readonly IThemeEditorService _themeEditor;
    private readonly IThemeCreator _themeCreator;
    private readonly ThemeOptions _themeOptions;
    private readonly string _zaggyQuestionPath;
    private readonly ILogger<AppearanceSettingsViewModel> _logger;
    private int _originalCodeFontSize;
    private int _originalTerminalFontSize;
    private bool _originalUseSystemTitleBar;
    private bool _originalShowSidebar;
    private SidebarPosition _originalSidebarPosition;
    private string _originalCodeTheme = string.Empty;
    private string _originalSelectedAppTheme = string.Empty;
    private ThemeCreationBehavior _originalThemeCreationBehavior;

    public int MinFontSize => _fontSizeOptions.MinFontSize;
    public int MaxFontSize => _fontSizeOptions.MaxFontSize;
    public int LoadingDotsMaxCount { get; }

    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public ObservableCollection<ThemeMetadataItem> AvailableAppThemes { get; } = [];
    public IReadOnlyList<ThemeCreationBehavior> ThemeCreationBehaviorValues { get; } = Enum.GetValues<ThemeCreationBehavior>();
    public IReadOnlyList<SidebarPosition> SidebarPositionValues { get; } = Enum.GetValues<SidebarPosition>();
    public Interaction<string, string> ApplyCodeThemeInteraction { get; } = new();
    public Interaction<ThemeEditorViewModel, Unit> OpenThemeEditor { get; } = new();
    public Interaction<AddThemeViewModel, string?> OpenAddTheme { get; } = new();
    public string CSharpExamplePath { get; }
    public string PythonExamplePath { get; }

    public AppearanceSettingsViewModel(
        IObservableStorage<UserData> userStorage,
        IObservableStorage<ThemeSettings> themeSettingsStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<DefaultThemeSettings> defaultThemeSettingsOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions,
        IOptions<LoadingOptions> loadingOptions,
        IThemeCatalog themeCatalog,
        IThemeCopier themeCopier,
        IThemeDeleter themeDeleter,
        IThemeEditorService themeEditor,
        IThemeCreator themeCreator,
        IOptions<ThemeOptions> themeOptions,
        IOptions<ZaggyAssetsOptions> zaggyAssets,
        ILogger<AppearanceSettingsViewModel> logger)
    {
        _fontSizeOptions = fontSizeOptions.Value;
        _defaultUserData = defaultUserOptions.Value.User;
        _userStorage = userStorage;
        _themeSettingsStorage = themeSettingsStorage;
        _defaultThemeSettings = defaultThemeSettingsOptions.Value.Settings;
        _themeCatalog = themeCatalog;
        _themeCopier = themeCopier;
        _themeDeleter = themeDeleter;
        _themeEditor = themeEditor;
        _themeCreator = themeCreator;
        _themeOptions = themeOptions.Value;
        _zaggyQuestionPath = zaggyAssets.Value.EmotionQuestion;
        _logger = logger;
        LoadingDotsMaxCount = loadingOptions.Value.LoadingDotsMaxCount;
        CSharpExamplePath = codeExamplePathOptions.Value.CSharpExamplePath;
        PythonExamplePath = codeExamplePathOptions.Value.PythonExamplePath;

        var current = userStorage.Current;
        _originalCodeFontSize = current.CodeFontSize;
        _originalTerminalFontSize = current.TerminalFontSize;
        _originalUseSystemTitleBar = current.UseSystemTitleBar;
        _originalShowSidebar = current.ShowSidebar;
        _originalSidebarPosition = current.SidebarPosition;
        _originalCodeTheme = current.CodeTheme;
        _originalSelectedAppTheme = themeSettingsStorage.Current.CurrentTheme;
        _originalThemeCreationBehavior = themeSettingsStorage.Current.OnCreateBehavior;

#pragma warning disable AsyncVoidMethod
        _ = InitializeAvailableAppThemesAsync();
#pragma warning restore AsyncVoidMethod

        LoadFromUserData(current);
        ThemeCreationBehavior = themeSettingsStorage.Current.OnCreateBehavior;
        HasChanges = false;

        InitializeMessageBusSubscriptions();

        this.WhenAnyPropertyChanged(
                nameof(CodeFontSize),
                nameof(TerminalFontSize),
                nameof(UseSystemTitleBar),
                nameof(ShowSidebar),
                nameof(SidebarPosition),
                nameof(SelectedCodeTheme),
                nameof(SelectedAppTheme),
                nameof(ThemeCreationBehavior))
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
        SidebarPosition = _defaultUserData.SidebarPosition;
        SelectedCodeTheme = _defaultUserData.CodeTheme;
        ThemeCreationBehavior = _defaultThemeSettings.OnCreateBehavior;

        ApplyAppThemeSelection(_defaultThemeSettings.CurrentTheme);
    }

    public void AcceptChanges()
    {
        _originalCodeFontSize = CodeFontSize;
        _originalTerminalFontSize = TerminalFontSize;
        _originalUseSystemTitleBar = UseSystemTitleBar;
        _originalShowSidebar = ShowSidebar;
        _originalSidebarPosition = SidebarPosition;
        _originalCodeTheme = SelectedCodeTheme;
        _originalSelectedAppTheme = SelectedAppTheme;
        _originalThemeCreationBehavior = ThemeCreationBehavior;

        _themeSettingsStorage.Current.CurrentTheme = SelectedAppTheme;
        _themeSettingsStorage.Current.OnCreateBehavior = ThemeCreationBehavior;

        HasChanges = false;
    }

    private void LoadFromUserData(UserData userData)
    {
        CodeFontSize = userData.CodeFontSize;
        TerminalFontSize = userData.TerminalFontSize;
        UseSystemTitleBar = userData.UseSystemTitleBar;
        ShowSidebar = userData.ShowSidebar;
        SidebarPosition = userData.SidebarPosition;
        SelectedCodeTheme = userData.CodeTheme;
    }

    private void UpdateHasChanges()
    {
        HasChanges =
            CodeFontSize != _originalCodeFontSize ||
            TerminalFontSize != _originalTerminalFontSize ||
            UseSystemTitleBar != _originalUseSystemTitleBar ||
            ShowSidebar != _originalShowSidebar ||
            SidebarPosition != _originalSidebarPosition ||
            SelectedCodeTheme != _originalCodeTheme ||
            SelectedAppTheme != _originalSelectedAppTheme ||
            ThemeCreationBehavior != _originalThemeCreationBehavior;
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
        ThemeCreationBehavior = _originalThemeCreationBehavior;
    }

    private async Task InitializeAvailableAppThemesAsync()
    {
        try
        {
            await LoadAppThemesAsync();
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

    private async Task ReloadAppThemesAsync()
    {
        try
        {
            await LoadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to reload application themes");
        }
    }

    private async Task LoadAppThemesAsync()
    {
        var themes = await Task.Run(async () => await _themeCatalog.GetAvailableThemesAsync());

        AvailableAppThemes.Clear();
        foreach (var metadata in themes)
        {
            var item = ThemeMetadataItem.From(metadata) with
            {
                CopyCommand = CopyThemeCommand,
                DeleteCommand = DeleteThemeCommand,
                EditCommand = EditThemeCommand
            };
            AvailableAppThemes.Add(item);
        }

        var saved = _themeSettingsStorage.Current.CurrentTheme;
        var selected = AvailableAppThemes.Any(theme => theme.Name == saved) ? saved : string.Empty;

        if (SelectedAppTheme != selected)
            SelectedAppTheme = selected;
    }

    [ReactiveCommand]
    private async Task CopyTheme(ThemeMetadataItem item)
    {
        try
        {
            await _themeCopier.CopyThemeAsync(item.Source);
            await ReloadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to copy theme '{name}'", item.Name);
        }
    }

    [ReactiveCommand]
    private async Task DeleteTheme(ThemeMetadataItem item)
    {
        try
        {
            await _themeDeleter.DeleteThemeAsync(item.Source);
            await ReloadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete theme '{name}'", item.Name);
        }
    }

    [ReactiveCommand]
    private async Task EditTheme(ThemeMetadataItem item)
    {
        if (item.IsSystemTheme)
            return;

        try
        {
            ApplyAppThemeSelection(item.Name);

            var editor = new ThemeEditorViewModel(_themeEditor);
            await editor.InitializeAsync(item.Name);
            await OpenThemeEditor.Handle(editor);
            await ReloadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to edit theme '{name}'", item.Name);
        }
    }

    [ReactiveCommand]
    private async Task AddNewTheme()
    {
        try
        {
            var existingNames = AvailableAppThemes.Select(theme => theme.Name).ToList();
            var viewModel = new AddThemeViewModel(existingNames, _zaggyQuestionPath);
            var name = await OpenAddTheme.Handle(viewModel);
            if (string.IsNullOrWhiteSpace(name))
                return;

            await CreateThemeAsync(name);
            await ReloadAppThemesAsync();

            if (ThemeCreationBehavior == ThemeCreationBehavior.Nothing)
                return;

            ApplyAppThemeSelection(name);

            if (ThemeCreationBehavior != ThemeCreationBehavior.OpenEditorAndApply)
                return;

            var editor = new ThemeEditorViewModel(_themeEditor);
            await editor.InitializeAsync(name);
            await OpenThemeEditor.Handle(editor);
            await ReloadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create a new theme");
        }
    }

    private async Task CreateThemeAsync(string name)
    {
        var source = await _themeCatalog.LoadThemeAsync("Primus")
            ?? throw new InvalidOperationException("Default theme 'Primus' was not found");

        var metadata = new ThemeMetadata
        {
            Name = name,
            Description = "Пользовательская тема",
            CreatedAtVersion = new Version(2026, 0, 0),
            Author = "Пользователь",
            WindowBackgroundColor = source.WindowBackgroundColor,
            SidebarBackgroundColor = source.SidebarBackgroundColor,
            EditorBackgroundColor = source.EditorBackgroundColor,
            TerminalBackgroundColor = source.TerminalBackgroundColor,
            AccentColor = source.AccentColor,
            BorderColor = source.BorderColor,
            TextPrimaryColor = source.TextPrimaryColor
        };

        Directory.CreateDirectory(_themeOptions.ExternThemesFolder);
        await _themeCreator.CreateAsync(source, metadata, _themeOptions.ExternThemesFolder);
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
