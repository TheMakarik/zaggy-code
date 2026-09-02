namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private int _selectedTabIndex;
    [Reactive] private bool _hasChanges;
    [Reactive] private bool _hasChangesAndPythonSettingsValid;

    #endregion

    private readonly IObservableStorage<UserData> _userStorage;
    private readonly IObservableStorage<PythonSettings> _pythonSettingsStorage;
    private readonly IObservableStorage<CSharpSettings> _csharpSettingsStorage;
    private readonly ILogger<SettingsViewModel> _logger;

    public AppearanceSettingsViewModel AppearanceSettings { get; }
    public PythonSettingsViewModel PythonSettings { get; }
    public CSharpSettingsViewModel CSharpSettings { get; }

    public Interaction<Unit, Unit> CloseSettingsInteraction { get; } = new();

    public SettingsViewModel(
        IObservableStorage<UserData> userStorage,
        IObservableStorage<PythonSettings> pythonSettingsStorage,
        IObservableStorage<CSharpSettings> csharpSettingsStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<PythonDefaultSettingsOptions> pythonDefaultSettingsOptions,
        IOptions<CSharpDefaultSettingsOptions> csharpDefaultSettingsOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions,
        IOptions<LoadingOptions> loadingOptions,
        IThemeCatalog themeCatalog,
        IPythonFunctionNameValidator pythonFunctionNameValidator,
        ILoggerFactory loggerFactory)
    {
        _userStorage = userStorage;
        _pythonSettingsStorage = pythonSettingsStorage;
        _csharpSettingsStorage = csharpSettingsStorage;
        _logger = loggerFactory.CreateLogger<SettingsViewModel>();

        AppearanceSettings = new AppearanceSettingsViewModel(
            userStorage,
            defaultUserOptions,
            codeExamplePathOptions,
            fontSizeOptions,
            loadingOptions,
            themeCatalog,
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
    private async Task SaveSettingsAsync()
    {
        _logger.LogInformation("Saving settings");

        var current = _userStorage.Current;
        current.CodeFontSize = AppearanceSettings.CodeFontSize;
        current.TerminalFontSize = AppearanceSettings.TerminalFontSize;
        current.UseSystemTitleBar = AppearanceSettings.UseSystemTitleBar;
        current.ShowSidebar = AppearanceSettings.ShowSidebar;
        current.CodeTheme = AppearanceSettings.SelectedCodeTheme;
        current.EnableCodeHighlighting = AppearanceSettings.EnableCodeHighlighting;
        current.ShowCodeLineNumbers = AppearanceSettings.ShowCodeLineNumbers;
        current.CurrentTheme = AppearanceSettings.SelectedAppTheme;

        var pythonCurrent = _pythonSettingsStorage.Current;
        pythonCurrent.UseEntryFunction = PythonSettings.UseEntryFunction;
        pythonCurrent.EntryFunctionName = PythonSettings.EntryFunctionName;
        pythonCurrent.SupressIo = PythonSettings.SupressIo;

        var csharpCurrent = _csharpSettingsStorage.Current;
        csharpCurrent.UseTopLevelStatements = CSharpSettings.UseTopLevelStatements;
        csharpCurrent.EnableImplicitUsings = CSharpSettings.EnableImplicitUsings;
        csharpCurrent.BlockIo = CSharpSettings.BlockIo;

        MessageBus.Current.SendMessage(new CodeFontSizeChangedMessage(AppearanceSettings.CodeFontSize));
        MessageBus.Current.SendMessage(new TerminalFontSizeChangedMessage(AppearanceSettings.TerminalFontSize));
        MessageBus.Current.SendMessage(new UseSystemTitleBarChangedMessage(AppearanceSettings.UseSystemTitleBar));
        MessageBus.Current.SendMessage(new ShowSidebarChangedMessage(AppearanceSettings.ShowSidebar));
        MessageBus.Current.SendMessage(new CodeThemeChangedMessage(AppearanceSettings.SelectedCodeTheme));
        MessageBus.Current.SendMessage(new EnableCodeHighlightingChangedMessage(AppearanceSettings.EnableCodeHighlighting));
        MessageBus.Current.SendMessage(new ShowCodeLineNumbersChangedMessage(AppearanceSettings.ShowCodeLineNumbers));
        MessageBus.Current.SendMessage(new AppThemeChangedMessage(AppearanceSettings.SelectedAppTheme));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("редактора", AppearanceSettings.CodeFontSize));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("терминала", AppearanceSettings.TerminalFontSize));

        AppearanceSettings.AcceptChanges();
        PythonSettings.AcceptChanges();
        CSharpSettings.AcceptChanges();
    }

    [ReactiveCommand]
    private async Task CancelSettingsAsync()
    {
        _logger.LogInformation("Cancelling settings changes");
        await CloseSettingsInteraction.Handle(Unit.Default);
    }

    public void RevertAppearanceChanges()
    {
        AppearanceSettings.RevertChanges();
    }

    [ReactiveCommand]
    private void ResetToDefaults()
    {
        AppearanceSettings.ResetToDefaults();
        PythonSettings.ResetToDefaults();
    }
}
