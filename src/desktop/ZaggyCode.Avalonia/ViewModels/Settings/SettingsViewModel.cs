namespace ZaggyCode.Avalonia.ViewModels.Settings;

public sealed partial class SettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private int _selectedTabIndex;
    [Reactive] private bool _hasChanges;
    [Reactive] private bool _hasChangesAndPythonSettingsValid;

    #endregion

    private readonly ISettingsStorage<ViewSettings> _viewStorage;
    private readonly ISettingsStorage<PythonSettings> _pythonSettingsStorage;
    private readonly ISettingsStorage<CSharpSettings> _csharpSettingsStorage;
    private readonly ISettingStorageFacade _storageFacade;
    private readonly ILogger<SettingsViewModel> _logger;

    public AppearanceSettingsViewModel AppearanceSettings { get; }
    public PythonSettingsViewModel PythonSettings { get; }
    public CSharpSettingsViewModel CSharpSettings { get; }

    public Interaction<Unit, Unit> CloseSettingsInteraction { get; } = new();

    public SettingsViewModel(
        ISettingsStorage<ViewSettings> viewStorage,
        ISettingsStorage<ThemeSettings> themeSettingsStorage,
        ISettingsStorage<PythonSettings> pythonSettingsStorage,
        ISettingsStorage<CSharpSettings> csharpSettingsStorage,
        ISettingStorageFacade storageFacade,
        IOptions<DefaultViewSettings> defaultViewSettingsOptions,
        IOptions<DefaultThemeSettings> defaultThemeSettingsOptions,
        IOptions<PythonDefaultSettingsOptions> pythonDefaultSettingsOptions,
        IOptions<CSharpDefaultSettingsOptions> csharpDefaultSettingsOptions,
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
        IPythonFunctionNameValidator pythonFunctionNameValidator,
        ILoggerFactory loggerFactory)
    {
        _viewStorage = viewStorage;
        _pythonSettingsStorage = pythonSettingsStorage;
        _csharpSettingsStorage = csharpSettingsStorage;
        _storageFacade = storageFacade;
        _logger = loggerFactory.CreateLogger<SettingsViewModel>();

        AppearanceSettings = new AppearanceSettingsViewModel(
            viewStorage,
            themeSettingsStorage,
            defaultViewSettingsOptions,
            defaultThemeSettingsOptions,
            codeExamplePathOptions,
            fontSizeOptions,
            loadingOptions,
            themeCatalog,
            themeCopier,
            themeDeleter,
            themeEditor,
            themeCreator,
            themeSearchService,
            themeOptions,
            zaggyAssets,
            loggerFactory.CreateLogger<AppearanceSettingsViewModel>());
        PythonSettings = new PythonSettingsViewModel(
            pythonSettingsStorage,
            pythonDefaultSettingsOptions,
            pythonFunctionNameValidator);
        CSharpSettings = new CSharpSettingsViewModel(
            csharpSettingsStorage,
            csharpDefaultSettingsOptions);

        AppearanceSettings.WhenAnyValue(viewModel => viewModel.HasChanges)
            .Subscribe(_ => UpdateChangeFlags());

        PythonSettings.WhenAnyValue(viewModel => viewModel.HasChanges)
            .Subscribe(_ => UpdateChangeFlags());

        CSharpSettings.WhenAnyValue(viewModel => viewModel.HasChanges)
            .Subscribe(_ => UpdateChangeFlags());

        PythonSettings.WhenAnyValue(viewModel => viewModel.IsSettingsValid)
            .Subscribe(_ => UpdateChangeFlags());

        UpdateChangeFlags();
    }

    private void UpdateChangeFlags()
    {
        HasChanges = AppearanceSettings.HasChanges || PythonSettings.HasChanges || CSharpSettings.HasChanges;
        HasChangesAndPythonSettingsValid = HasChanges && PythonSettings.IsSettingsValid;
    }

    [ReactiveCommand]
    private void SaveSettings()
    {
        _logger.LogInformation("Сохранение настроек");

        if (AppearanceSettings.HasChanges)
            SaveAppearanceSettings();

        if (PythonSettings.HasChanges)
            SavePythonSettings();

        if (CSharpSettings.HasChanges)
            SaveCSharpSettings();

        _storageFacade.FlashAllAtBackground();
        MessageBus.Current.SendMessage(new SettingsSavedMessage());
    }

    private void SaveAppearanceSettings()
    {
        var current = _viewStorage.Value;
        var codeFontSizeChanged = current.CodeFontSize != AppearanceSettings.CodeFontSize;
        var terminalFontSizeChanged = current.TerminalFontSize != AppearanceSettings.TerminalFontSize;
        current.CodeFontSize = AppearanceSettings.CodeFontSize;
        current.TerminalFontSize = AppearanceSettings.TerminalFontSize;
        current.UseSystemTitleBar = AppearanceSettings.UseSystemTitleBar;
        current.SidebarPosition = AppearanceSettings.SidebarPosition;
        current.CodeTheme = AppearanceSettings.SelectedCodeTheme;

        AppearanceSettings.AcceptChanges();

        MessageBus.Current.SendMessage(new CodeFontSizeChangedMessage(AppearanceSettings.CodeFontSize));
        MessageBus.Current.SendMessage(new TerminalFontSizeChangedMessage(AppearanceSettings.TerminalFontSize));
        MessageBus.Current.SendMessage(new UseSystemTitleBarChangedMessage(AppearanceSettings.UseSystemTitleBar));
        MessageBus.Current.SendMessage(new SidebarPositionChangedMessage(AppearanceSettings.SidebarPosition));
        MessageBus.Current.SendMessage(new CodeThemeChangedMessage(AppearanceSettings.SelectedCodeTheme));
        MessageBus.Current.SendMessage(new AppThemeChangedMessage(AppearanceSettings.SelectedAppTheme));
        if (codeFontSizeChanged)
            MessageBus.Current.SendMessage(new FontSizeToastMessage("редактора и описания", AppearanceSettings.CodeFontSize));
        if (terminalFontSizeChanged)
            MessageBus.Current.SendMessage(new FontSizeToastMessage("терминала", AppearanceSettings.TerminalFontSize));
    }

    private void SavePythonSettings()
    {
        var current = _pythonSettingsStorage.Value;
        current.UseEntryFunction = PythonSettings.UseEntryFunction;
        current.EntryFunctionName = PythonSettings.EntryFunctionName;
        current.SupressIo = PythonSettings.SupressIo;

        PythonSettings.AcceptChanges();
    }

    private void SaveCSharpSettings()
    {
        var current = _csharpSettingsStorage.Value;
        current.UseTopLevelStatements = CSharpSettings.UseTopLevelStatements;
        current.EnableImplicitUsings = CSharpSettings.EnableImplicitUsings;
        current.BlockIo = CSharpSettings.BlockIo;

        CSharpSettings.AcceptChanges();
    }

    [ReactiveCommand]
    private async Task CancelSettingsAsync()
    {
        _logger.LogInformation("Отмена изменений настроек");
        await CloseSettingsInteraction.Handle(Unit.Default);
    }

    public void RevertAppearanceChanges()
    {
        AppearanceSettings.RevertChanges();
    }

    [ReactiveCommand]
    private void ResetToDefaults()
    {
        _storageFacade.ToDefaultAllAtBackground();
        AppearanceSettings.ResetToDefaults();
        PythonSettings.ResetToDefaults();
        MessageBus.Current.SendMessage(new SettingsResetMessage());
    }
}
