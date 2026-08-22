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
    private readonly ILogger<SettingsViewModel> _logger;

    public AppearanceSettingsViewModel AppearanceSettings { get; }
    public PythonSettingsViewModel PythonSettings { get; }
    public CSharpSettingsViewModel CSharpSettings { get; }

    public Interaction<Unit, Unit> CloseSettingsInteraction { get; } = new();

    public SettingsViewModel(
        IObservableStorage<UserData> userStorage,
        IObservableStorage<PythonSettings> pythonSettingsStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<PythonDefaultSettingsOptions> pythonDefaultSettingsOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions,
        IPythonFunctionNameValidator pythonFunctionNameValidator,
        ILoggerFactory loggerFactory)
    {
        _userStorage = userStorage;
        _pythonSettingsStorage = pythonSettingsStorage;
        _logger = loggerFactory.CreateLogger<SettingsViewModel>();

        AppearanceSettings = new AppearanceSettingsViewModel(
            userStorage,
            defaultUserOptions,
            codeExamplePathOptions,
            fontSizeOptions,
            loggerFactory.CreateLogger<AppearanceSettingsViewModel>());
        PythonSettings = new PythonSettingsViewModel(
            pythonSettingsStorage,
            pythonDefaultSettingsOptions,
            pythonFunctionNameValidator);
        CSharpSettings = new CSharpSettingsViewModel();

        AppearanceSettings.WhenAnyValue(viewModel => viewModel.HasChanges)
            .Subscribe(_ => UpdateChangeFlags());

        PythonSettings.WhenAnyValue(viewModel => viewModel.HasChanges)
            .Subscribe(_ => UpdateChangeFlags());

        PythonSettings.WhenAnyValue(viewModel => viewModel.IsSettingsValid)
            .Subscribe(_ => UpdateChangeFlags());

        UpdateChangeFlags();
    }

    private void UpdateChangeFlags()
    {
        HasChanges = AppearanceSettings.HasChanges || PythonSettings.HasChanges;
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

        var pythonCurrent = _pythonSettingsStorage.Current;
        pythonCurrent.UseEntryFunction = PythonSettings.UseEntryFunction;
        pythonCurrent.EntryFunctionName = PythonSettings.EntryFunctionName;
        pythonCurrent.SupressIo = PythonSettings.SupressIo;

        MessageBus.Current.SendMessage(new CodeFontSizeChangedMessage(AppearanceSettings.CodeFontSize));
        MessageBus.Current.SendMessage(new TerminalFontSizeChangedMessage(AppearanceSettings.TerminalFontSize));
        MessageBus.Current.SendMessage(new UseSystemTitleBarChangedMessage(AppearanceSettings.UseSystemTitleBar));
        MessageBus.Current.SendMessage(new ShowSidebarChangedMessage(AppearanceSettings.ShowSidebar));
        MessageBus.Current.SendMessage(new CodeThemeChangedMessage(AppearanceSettings.SelectedCodeTheme));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("редактора", AppearanceSettings.CodeFontSize));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("терминала", AppearanceSettings.TerminalFontSize));

        AppearanceSettings.AcceptChanges();
        PythonSettings.AcceptChanges();
    }

    [ReactiveCommand]
    private async Task CancelSettingsAsync()
    {
        _logger.LogInformation("Cancelling settings changes");
        await CloseSettingsInteraction.Handle(Unit.Default);
    }

    [ReactiveCommand]
    private void ResetToDefaults()
    {
        AppearanceSettings.ResetToDefaults();
        PythonSettings.ResetToDefaults();
    }
}
