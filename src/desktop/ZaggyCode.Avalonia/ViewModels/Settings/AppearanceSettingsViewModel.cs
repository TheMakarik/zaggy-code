namespace ZaggyCode.Avalonia.ViewModels.Settings;

public sealed partial class AppearanceSettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _isLoadingThemes = true;
    [Reactive] private string _themeSearchText = string.Empty;
    [Reactive] private bool _canDecreaseCodeFontSize;
    [Reactive] private bool _canIncreaseCodeFontSize;
    [Reactive] private bool _canDecreaseTerminalFontSize;
    [Reactive] private bool _canIncreaseTerminalFontSize;

    #endregion

    private readonly FontSizeOptions _fontSizeOptions;
    private readonly ISettingsStorage<ViewSettings> _viewStorage;
    private readonly ISettingsStorage<ThemeSettings> _themeSettingsStorage;
    private readonly IThemeCatalog _themeCatalog;
    private readonly IThemeCopier _themeCopier;
    private readonly IThemeDeleter _themeDeleter;
    private readonly IThemeEditorService _themeEditor;
    private readonly IThemeCreator _themeCreator;
    private readonly IThemeSearchService _themeSearchService;
    private readonly ThemeOptions _themeOptions;
    private readonly string _zaggyQuestionPath;
    private readonly ILogger<AppearanceSettingsViewModel> _logger;

    public int MinFontSize => _fontSizeOptions.MinFontSize;
    public int MaxFontSize => _fontSizeOptions.MaxFontSize;
    public int LoadingDotsMaxCount { get; }
    public bool HasChanges => _viewStorage.HasChanges || _themeSettingsStorage.HasChanges;

    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public ObservableCollection<ThemeMetadataItem> AvailableAppThemes { get; } = [];
    public ObservableCollection<ThemeMetadataItem> FilteredAppThemes { get; } = [];
    public IReadOnlyList<ThemeCreationBehavior> ThemeCreationBehaviorValues { get; } = Enum.GetValues<ThemeCreationBehavior>();
    public IReadOnlyList<SidebarPosition> SidebarPositionValues { get; } = Enum.GetValues<SidebarPosition>();
    public Interaction<string, string> ApplyCodeThemeInteraction { get; } = new();
    public Interaction<ThemeEditorViewModel, Unit> OpenThemeEditor { get; } = new();
    public Interaction<AddThemeViewModel, string?> OpenAddTheme { get; } = new();
    public Interaction<string, Unit> ShowThemeFileInteraction { get; } = new();
    public Interaction<string, Unit> CopyThemeFileInteraction { get; } = new();
    public string CSharpExamplePath { get; }
    public string PythonExamplePath { get; }

    public AppearanceSettingsViewModel(
        ISettingsStorage<ViewSettings> viewStorage,
        ISettingsStorage<ThemeSettings> themeSettingsStorage,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions,
        IOptions<LoadingOptions> loadingOptions,
        IThemeCatalog themeCatalog,
        IThemeCopier themeCopier,
        IThemeDeleter themeDeleter,
        IThemeEditorService themeEditor,
        IThemeCreator themeCreator,
        IThemeSearchService themeSearchService,
        IOptions<ThemeOptions> themeOptions,
        IOptions<ZaggyAssetsOptions> zaggyAssets,
        ILogger<AppearanceSettingsViewModel> logger)
    {
        _viewStorage = viewStorage;
        _themeSettingsStorage = themeSettingsStorage;
        _fontSizeOptions = fontSizeOptions.Value;
        _themeCatalog = themeCatalog;
        _themeCopier = themeCopier;
        _themeDeleter = themeDeleter;
        _themeEditor = themeEditor;
        _themeCreator = themeCreator;
        _themeSearchService = themeSearchService;
        _themeOptions = themeOptions.Value;
        _zaggyQuestionPath = zaggyAssets.Value.EmotionQuestion;
        _logger = logger;
        LoadingDotsMaxCount = loadingOptions.Value.LoadingDotsMaxCount;
        CSharpExamplePath = codeExamplePathOptions.Value.CSharpExamplePath;
        PythonExamplePath = codeExamplePathOptions.Value.PythonExamplePath;

#pragma warning disable AsyncVoidMethod
        _ = InitializeAvailableAppThemesAsync();
#pragma warning restore AsyncVoidMethod

        InitializeMessageBusSubscriptions();
        ObserveSettingsStorages();
        UpdateFontSizeLimits();
    }

    #region Settings proxies

    public int CodeFontSize
    {
        get => _viewStorage.Value.CodeFontSize;
        set
        {
            if (value == _viewStorage.Value.CodeFontSize)
                return;

            _viewStorage.Value.CodeFontSize = value;
            this.RaisePropertyChanged();
            UpdateFontSizeLimits();
        }
    }

    public int TerminalFontSize
    {
        get => _viewStorage.Value.TerminalFontSize;
        set
        {
            if (value == _viewStorage.Value.TerminalFontSize)
                return;

            _viewStorage.Value.TerminalFontSize = value;
            this.RaisePropertyChanged();
            UpdateFontSizeLimits();
        }
    }

    public bool UseSystemTitleBar
    {
        get => _viewStorage.Value.UseSystemTitleBar;
        set => SetViewSetting(settings => settings.UseSystemTitleBar = value);
    }

    public SidebarPosition SidebarPosition
    {
        get => _viewStorage.Value.SidebarPosition;
        set => SetViewSetting(settings => settings.SidebarPosition = value);
    }

    public string SelectedCodeTheme
    {
        get => _viewStorage.Value.CodeTheme;
        set
        {
            if (value == _viewStorage.Value.CodeTheme)
                return;

            _viewStorage.Value.CodeTheme = value;
            this.RaisePropertyChanged();
            MessageBus.Current.SendMessage(new CodeThemeChangedMessage(value));
        }
    }

    public string SelectedAppTheme
    {
        get => _themeSettingsStorage.Value.CurrentTheme;
        set
        {
            if (value == _themeSettingsStorage.Value.CurrentTheme)
                return;

            _themeSettingsStorage.Value.CurrentTheme = value;
            this.RaisePropertyChanged();
            MessageBus.Current.SendMessage(new AppThemeChangedMessage(value));
        }
    }

    public ThemeCreationBehavior ThemeCreationBehavior
    {
        get => _themeSettingsStorage.Value.OnCreateBehavior;
        set
        {
            if (value == _themeSettingsStorage.Value.OnCreateBehavior)
                return;

            _themeSettingsStorage.Value.OnCreateBehavior = value;
            this.RaisePropertyChanged();
        }
    }

    #endregion

    private void SetViewSetting(Action<ViewSettings> update, [CallerMemberName] string? propertyName = null)
    {
        update(_viewStorage.Value);
        this.RaisePropertyChanged(propertyName);
    }

    private void ObserveSettingsStorages()
    {
        _viewStorage.Value.PropertyChanged += (_, args) =>
        {
            this.RaisePropertyChanged(nameof(HasChanges));
            ApplyViewSettingsChange(args.PropertyName);
        };

        _themeSettingsStorage.Value.PropertyChanged += (_, args) =>
        {
            this.RaisePropertyChanged(nameof(HasChanges));
            ApplyThemeSettingsChange(args.PropertyName);
        };
    }

    private void ApplyViewSettingsChange(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(ViewSettings.CodeFontSize):
            case nameof(ViewSettings.TerminalFontSize):
                UpdateFontSizeLimits();
                break;
            case nameof(ViewSettings.CodeTheme):
                this.RaisePropertyChanged(nameof(SelectedCodeTheme));
                MessageBus.Current.SendMessage(new CodeThemeChangedMessage(_viewStorage.Value.CodeTheme));
                break;
        }

        this.RaisePropertyChanged(propertyName);
    }

    private void ApplyThemeSettingsChange(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(ThemeSettings.CurrentTheme):
                this.RaisePropertyChanged(nameof(SelectedAppTheme));
                MessageBus.Current.SendMessage(new AppThemeChangedMessage(_themeSettingsStorage.Value.CurrentTheme));
                break;
            case nameof(ThemeSettings.OnCreateBehavior):
                this.RaisePropertyChanged(nameof(ThemeCreationBehavior));
                break;
        }
    }

    private void UpdateFontSizeLimits()
    {
        CanDecreaseCodeFontSize = CodeFontSize > MinFontSize;
        CanIncreaseCodeFontSize = CodeFontSize < MaxFontSize;
        CanDecreaseTerminalFontSize = TerminalFontSize > MinFontSize;
        CanIncreaseTerminalFontSize = TerminalFontSize < MaxFontSize;
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

        this.WhenAnyValue(viewModel => viewModel.ThemeSearchText)
            .Subscribe(_ => ApplyThemeFilter());

        MessageBus.Current.SendMessage(new CodeThemesRequestMessage());
    }

    [ReactiveCommand]
    private void SelectCodeTheme(string themeName)
        => SelectedCodeTheme = themeName;

    [ReactiveCommand]
    private void SelectSidebarPosition(SidebarPosition position)
        => SidebarPosition = position;

    [ReactiveCommand]
    private void SelectAppTheme(string themeName)
        => SelectedAppTheme = themeName;

    private async Task InitializeAvailableAppThemesAsync()
    {
        try
        {
            await LoadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось загрузить темы приложения");
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
            _logger.LogError(e, "Не удалось перезагрузить темы приложения");
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
                EditCommand = EditThemeCommand,
                ShowFileCommand = ShowThemeFileCommand,
                CopyFileCommand = CopyThemeFileCommand,
                SelectCommand = SelectAppThemeCommand,
                CreateNewCommand = AddNewThemeCommand
            };
            AvailableAppThemes.Add(item);
        }

        ApplyThemeFilter();
    }

    private void ApplyThemeFilter()
    {
        var itemByMetadata = AvailableAppThemes.ToDictionary(item => item.Source);
        var filtered = _themeSearchService.Search([.. itemByMetadata.Keys], ThemeSearchText);

        FilteredAppThemes.Clear();
        foreach (var metadata in filtered)
            FilteredAppThemes.Add(itemByMetadata[metadata]);
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
            _logger.LogError(e, "Не удалось скопировать тему «{name}»", item.Name);
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
            _logger.LogError(e, "Не удалось удалить тему «{name}»", item.Name);
        }
    }

    [ReactiveCommand]
    private async Task EditTheme(ThemeMetadataItem item)
    {
        if (item.IsSystemTheme)
            return;

        try
        {
            SelectedAppTheme = item.Name;

            var editor = new ThemeEditorViewModel(_themeEditor);
            await editor.InitializeAsync(item.Name);
            await OpenThemeEditor.Handle(editor);
            await ReloadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось начать редактирование темы «{name}»", item.Name);
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

            SelectedAppTheme = name;

            if (ThemeCreationBehavior != ThemeCreationBehavior.OpenEditorAndApply)
                return;

            var editor = new ThemeEditorViewModel(_themeEditor);
            await editor.InitializeAsync(name);
            await OpenThemeEditor.Handle(editor);
            await ReloadAppThemesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось создать новую тему");
        }
    }

    [ReactiveCommand]
    private async Task ShowThemeFile(ThemeMetadataItem item)
    {
        if (item.Source.Path is not { } path)
        {
            _logger.LogWarning("Тема «{name}» не имеет файла на диске", item.Name);
            return;
        }

        await ShowThemeFileInteraction.Handle(path);
    }

    [ReactiveCommand]
    private async Task CopyThemeFile(ThemeMetadataItem item)
    {
        if (item.Source.Path is not { } path)
        {
            _logger.LogWarning("Тема «{name}» не имеет файла на диске", item.Name);
            return;
        }

        await CopyThemeFileInteraction.Handle(path);
    }

    private async Task CreateThemeAsync(string name)
    {
        var source = await _themeCatalog.LoadThemeAsync("Primus")
            ?? throw new InvalidOperationException("Тема по умолчанию «Primus» не найдена");

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
            _logger.LogWarning("Не удалось разобрать изменение размера шрифта редактора: {DeltaText}", deltaText);
            return;
        }

        CodeFontSize = Math.Clamp(CodeFontSize + delta, _fontSizeOptions.MinFontSize, _fontSizeOptions.MaxFontSize);
    }

    [ReactiveCommand]
    private void ChangeTerminalFontSize(string? deltaText)
    {
        if (!int.TryParse(deltaText, out var delta))
        {
            _logger.LogWarning("Не удалось разобрать изменение размера шрифта терминала: {DeltaText}", deltaText);
            return;
        }

        TerminalFontSize = Math.Clamp(TerminalFontSize + delta, _fontSizeOptions.MinFontSize, _fontSizeOptions.MaxFontSize);
    }
}
