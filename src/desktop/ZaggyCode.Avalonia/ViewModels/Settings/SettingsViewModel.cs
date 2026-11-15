namespace ZaggyCode.Avalonia.ViewModels.Settings;

public sealed partial class SettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private int _selectedTabIndex;
    [Reactive] private bool _hasChanges;
    [Reactive] private bool _hasChangesAndPythonSettingsValid;

    #endregion

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
        _storageFacade = storageFacade;
        _logger = loggerFactory.CreateLogger<SettingsViewModel>();

        AppearanceSettings = new AppearanceSettingsViewModel(
            viewStorage,
            themeSettingsStorage,
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
            pythonFunctionNameValidator);
        CSharpSettings = new CSharpSettingsViewModel(csharpSettingsStorage);

        viewStorage.Value.PropertyChanged += (_, _) => UpdateChangeFlags();
        themeSettingsStorage.Value.PropertyChanged += (_, _) => UpdateChangeFlags();
        pythonSettingsStorage.Value.PropertyChanged += (_, _) => UpdateChangeFlags();
        csharpSettingsStorage.Value.PropertyChanged += (_, _) => UpdateChangeFlags();

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
        _storageFacade.FlashAllAtBackground();
        MessageBus.Current.SendMessage(new SettingsSavedMessage());
    }

    [ReactiveCommand]
    private async Task CancelSettingsAsync()
    {
        _logger.LogInformation("Отмена изменений настроек");
        RevertAllChanges();
        await CloseSettingsInteraction.Handle(Unit.Default);
    }

    public void RevertAllChanges()
        => _storageFacade.RevertAllAtBackground();

    [ReactiveCommand]
    private void ResetToDefaults()
    {
        _storageFacade.ToDefaultAllAtBackground();
        MessageBus.Current.SendMessage(new SettingsResetMessage());
    }
}
