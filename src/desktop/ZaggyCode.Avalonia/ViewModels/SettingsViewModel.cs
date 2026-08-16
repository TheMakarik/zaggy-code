namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    [Reactive] private int _selectedTabIndex;
    [Reactive] private int _codeFontSize;
    [Reactive] private int _terminalFontSize;
    [Reactive] private bool _useSystemTitleBar;
    [Reactive] private bool _showSidebar;
    [Reactive] private string _selectedCodeTheme = string.Empty;
    [Reactive] private bool _hasChanges;
    [Reactive] private bool _hasChangesAndPythonSettingsValid;
    [Reactive] private bool _canDecreaseCodeFontSize;
    [Reactive] private bool _canIncreaseCodeFontSize;
    [Reactive] private bool _canDecreaseTerminalFontSize;
    [Reactive] private bool _canIncreaseTerminalFontSize;

    [Reactive] private bool _useEntryFunction;
    [Reactive] private string _entryFunctionName = string.Empty;
    [Reactive] private bool _supressIo;
    [Reactive] private PythonFunctionNameValidationResult _entryFunctionNameValidationResult;
    [Reactive] private bool _isPythonSettingsValid;

    private readonly IObservableStorage<UserData> _userStorage;
    private readonly IObservableStorage<PythonSettings> _pythonSettingsStorage;
    private readonly DefaultUser _defaultUser;
    private readonly PythonSettings _defaultPythonSettings;
    private readonly FontSizeOptions _fontSizeOptions;
    private readonly IPythonFunctionNameValidator _pythonFunctionNameValidator;
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly int _originalCodeFontSize;
    private readonly int _originalTerminalFontSize;
    private readonly bool _originalUseSystemTitleBar;
    private readonly bool _originalShowSidebar;
    private readonly string _originalCodeTheme;
    private readonly bool _originalUseEntryFunction;
    private readonly string _originalEntryFunctionName;
    private readonly bool _originalDetailedExceptions;
    private readonly bool _originalSupressIo;

    public int MinFontSize => _fontSizeOptions.MinFontSize;
    public int MaxFontSize => _fontSizeOptions.MaxFontSize;

    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public Interaction<Unit, Unit> CloseSettingsInteraction { get; } = new();
    public Interaction<string, string> ApplyCodeThemeInteraction { get; } = new();
    public string CSharpExamplePath { get; }
    public string PythonExamplePath { get; }

    public SettingsViewModel(
        IObservableStorage<UserData> userStorage,
        IObservableStorage<PythonSettings> pythonSettingsStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<PythonDefaultSettingsOptions> pythonDefaultSettingsOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions,
        IPythonFunctionNameValidator pythonFunctionNameValidator,
        ILogger<SettingsViewModel> logger)
    {
        _userStorage = userStorage;
        _pythonSettingsStorage = pythonSettingsStorage;
        _defaultUser = defaultUserOptions.Value;
        _defaultPythonSettings = pythonDefaultSettingsOptions.Value.Settings;
        _fontSizeOptions = fontSizeOptions.Value;
        _pythonFunctionNameValidator = pythonFunctionNameValidator;
        _logger = logger;
        CSharpExamplePath = codeExamplePathOptions.Value.CSharpExamplePath;
        PythonExamplePath = codeExamplePathOptions.Value.PythonExamplePath;

        InitializeMessageBusSubscriptions();

        var current = userStorage.Current;
        _originalCodeFontSize = current.CodeFontSize;
        _originalTerminalFontSize = current.TerminalFontSize;
        _originalUseSystemTitleBar = current.UseSystemTitleBar;
        _originalShowSidebar = current.ShowSidebar;
        _originalCodeTheme = current.CodeTheme;

        var pythonCurrent = pythonSettingsStorage.Current;
        _originalUseEntryFunction = pythonCurrent.UseEntryFunction;
        _originalEntryFunctionName = pythonCurrent.EntryFunctionName;
        _originalSupressIo = pythonCurrent.SupressIo;

        LoadFromUserData();
        LoadFromPythonSettings();
        UpdateEntryFunctionNameValidation();
        HasChanges = false;

        this.WhenAnyPropertyChanged(
                nameof(CodeFontSize),
                nameof(TerminalFontSize),
                nameof(UseSystemTitleBar),
                nameof(ShowSidebar),
                nameof(SelectedCodeTheme),
                nameof(UseEntryFunction),
                nameof(EntryFunctionName),
                nameof(SupressIo))
            .Subscribe(_ => UpdateHasChanges());

        this.WhenAnyValue(viewModel => viewModel.UseEntryFunction)
            .Subscribe(_ => UpdateEntryFunctionNameValidation());

        this.WhenAnyValue(viewModel => viewModel.EntryFunctionName)
            .Subscribe(_ => UpdateEntryFunctionNameValidation());

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

    private void LoadFromUserData()
    {
        var current = _userStorage.Current;
        CodeFontSize = current.CodeFontSize;
        TerminalFontSize = current.TerminalFontSize;
        UseSystemTitleBar = current.UseSystemTitleBar;
        ShowSidebar = current.ShowSidebar;
        SelectedCodeTheme = current.CodeTheme;
    }

    private void LoadFromPythonSettings()
    {
        var current = _pythonSettingsStorage.Current;
        UseEntryFunction = current.UseEntryFunction;
        EntryFunctionName = current.EntryFunctionName;
        SupressIo = current.SupressIo;
    }

    private void UpdateEntryFunctionNameValidation()
    {
        if (!UseEntryFunction)
        {
            EntryFunctionNameValidationResult = PythonFunctionNameValidationResult.Success;
            IsPythonSettingsValid = true;
            return;
        }

        EntryFunctionNameValidationResult = _pythonFunctionNameValidator.Validate(EntryFunctionName);
        IsPythonSettingsValid = EntryFunctionNameValidationResult == PythonFunctionNameValidationResult.Success;
    }

    private void UpdateHasChanges()
    {
        var hasChanges =
            CodeFontSize != _originalCodeFontSize ||
            TerminalFontSize != _originalTerminalFontSize ||
            UseSystemTitleBar != _originalUseSystemTitleBar ||
            ShowSidebar != _originalShowSidebar ||
            SelectedCodeTheme != _originalCodeTheme ||
            UseEntryFunction != _originalUseEntryFunction ||
            EntryFunctionName != _originalEntryFunctionName ||
            SupressIo != _originalSupressIo;

        HasChanges = hasChanges;
        HasChangesAndPythonSettingsValid = hasChanges && IsPythonSettingsValid;
    }

    [ReactiveCommand]
    private async Task SaveSettingsAsync()
    {
        _logger.LogInformation("Saving settings");

        var current = _userStorage.Current;
        current.CodeFontSize = CodeFontSize;
        current.TerminalFontSize = TerminalFontSize;
        current.UseSystemTitleBar = UseSystemTitleBar;
        current.ShowSidebar = ShowSidebar;
        current.CodeTheme = SelectedCodeTheme;

        var pythonCurrent = _pythonSettingsStorage.Current;
        pythonCurrent.UseEntryFunction = UseEntryFunction;
        pythonCurrent.EntryFunctionName = EntryFunctionName;
        pythonCurrent.SupressIo = SupressIo;

        MessageBus.Current.SendMessage(new CodeFontSizeChangedMessage(CodeFontSize));
        MessageBus.Current.SendMessage(new TerminalFontSizeChangedMessage(TerminalFontSize));
        MessageBus.Current.SendMessage(new UseSystemTitleBarChangedMessage(UseSystemTitleBar));
        MessageBus.Current.SendMessage(new ShowSidebarChangedMessage(ShowSidebar));
        MessageBus.Current.SendMessage(new CodeThemeChangedMessage(SelectedCodeTheme));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("редактора", CodeFontSize));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("терминала", TerminalFontSize));

        HasChanges = false;
       
    }

    [ReactiveCommand]
    private async Task CancelSettingsAsync()
    {
        _logger.LogInformation("Cancelling settings changes");
        await CloseSettingsInteraction.Handle(Unit.Default);
    }

    [ReactiveCommand]
    private void SelectCodeTheme(string themeName)
    {
        SelectedCodeTheme = themeName;
    }

    [ReactiveCommand]
    private void ResetToDefaults()
    {
        var defaultUserData = _defaultUser.User;
        CodeFontSize = defaultUserData.CodeFontSize;
        TerminalFontSize = defaultUserData.TerminalFontSize;
        UseSystemTitleBar = defaultUserData.UseSystemTitleBar;
        ShowSidebar = defaultUserData.ShowSidebar;
        SelectedCodeTheme = defaultUserData.CodeTheme;

        UseEntryFunction = _defaultPythonSettings.UseEntryFunction;
        EntryFunctionName = _defaultPythonSettings.EntryFunctionName;
        SupressIo = _defaultPythonSettings.SupressIo;
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
