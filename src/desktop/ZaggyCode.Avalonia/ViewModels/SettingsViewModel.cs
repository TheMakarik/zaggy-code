
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

    private readonly IUserStorage _userStorage;
    private readonly DefaultUser _defaultUser;
    private readonly FontSizeOptions _fontSizeOptions;
    private readonly CodeThemeDisplayNameOptions _displayNameOptions;
    private readonly CodeThemeIconOptions _iconOptions;
    private readonly int _originalCodeFontSize;
    private readonly int _originalTerminalFontSize;
    private readonly bool _originalUseSystemTitleBar;
    private readonly bool _originalShowSidebar;
    private readonly string _originalCodeTheme;

    public int MinFontSize => _fontSizeOptions.MinFontSize;
    public int MaxFontSize => _fontSizeOptions.MaxFontSize;

    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public Interaction<Unit, Unit> CloseSettingsInteraction { get; } = new();
    public Interaction<string, string> ApplyCodeThemeInteraction { get; } = new();
    public string CSharpExamplePath { get; }
    public string PythonExamplePath { get; }

    public SettingsViewModel(
        IUserStorage userStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions,
        IOptions<CodeThemeDisplayNameOptions> displayNameOptions,
        IOptions<CodeThemeIconOptions> iconOptions)
    {
        _userStorage = userStorage;
        _defaultUser = defaultUserOptions.Value;
        _fontSizeOptions = fontSizeOptions.Value;
        _displayNameOptions = displayNameOptions.Value;
        _iconOptions = iconOptions.Value;
        CSharpExamplePath = codeExamplePathOptions.Value.CSharpExamplePath;
        PythonExamplePath = codeExamplePathOptions.Value.PythonExamplePath;

        InitializeAvailableCodeThemes();

        var current = userStorage.Current;
        _originalCodeFontSize = current.CodeFontSize;
        _originalTerminalFontSize = current.TerminalFontSize;
        _originalUseSystemTitleBar = current.UseSystemTitleBar;
        _originalShowSidebar = current.ShowSidebar;
        _originalCodeTheme = current.CodeTheme;

        LoadFromUserData();
        HasChanges = false;

        this.WhenAnyPropertyChanged(
                nameof(CodeFontSize),
                nameof(TerminalFontSize),
                nameof(UseSystemTitleBar),
                nameof(ShowSidebar),
                nameof(SelectedCodeTheme))
            .Subscribe(_ => UpdateHasChanges());

        this.WhenAnyValue(viewModel => viewModel.SelectedCodeTheme)
            .Skip(1)
            .SelectMany(themeName => ApplyCodeThemeInteraction.Handle(themeName))
            .Subscribe();
    }

    private void InitializeAvailableCodeThemes()
    {
        foreach (var property in typeof(CodeThemeDisplayNameOptions).GetProperties())
        {
            var themeName = property.Name;
            var displayName = (string?)property.GetValue(_displayNameOptions) ?? themeName;

            var iconProperty = typeof(CodeThemeIconOptions).GetProperty(themeName);
            var iconName = (string?)iconProperty?.GetValue(_iconOptions);
            var iconKind = Enum.TryParse<MaterialIconKind>(iconName, out var kind) ? kind : MaterialIconKind.Palette;

            AvailableCodeThemes.Add(new CodeThemeItem(themeName, displayName, iconKind));
        }
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

    private void UpdateHasChanges()
    {
        HasChanges =
            CodeFontSize != _originalCodeFontSize ||
            TerminalFontSize != _originalTerminalFontSize ||
            UseSystemTitleBar != _originalUseSystemTitleBar ||
            ShowSidebar != _originalShowSidebar ||
            SelectedCodeTheme != _originalCodeTheme;
    }

    [ReactiveCommand]
    private async Task SaveSettingsAsync()
    {
        var current = _userStorage.Current;
        current.CodeFontSize = CodeFontSize;
        current.TerminalFontSize = TerminalFontSize;
        current.UseSystemTitleBar = UseSystemTitleBar;
        current.ShowSidebar = ShowSidebar;
        current.CodeTheme = SelectedCodeTheme;

        MessageBus.Current.SendMessage(new CodeFontSizeChangedMessage(CodeFontSize));
        MessageBus.Current.SendMessage(new TerminalFontSizeChangedMessage(TerminalFontSize));
        MessageBus.Current.SendMessage(new UseSystemTitleBarChangedMessage(UseSystemTitleBar));
        MessageBus.Current.SendMessage(new ShowSidebarChangedMessage(ShowSidebar));
        MessageBus.Current.SendMessage(new CodeThemeChangedMessage(SelectedCodeTheme));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("редактора", CodeFontSize));
        MessageBus.Current.SendMessage(new FontSizeToastMessage("терминала", TerminalFontSize));

        HasChanges = false;
        await CloseSettingsInteraction.Handle(Unit.Default);
    }

    [ReactiveCommand]
    private async Task CancelSettingsAsync()
    {
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
    }

    [ReactiveCommand]
    private void ChangeCodeFontSize(int delta)
    {
        CodeFontSize = Math.Clamp(CodeFontSize + delta, _fontSizeOptions.MinFontSize, _fontSizeOptions.MaxFontSize);
    }

    [ReactiveCommand]
    private void ChangeTerminalFontSize(int delta)
    {
        TerminalFontSize = Math.Clamp(TerminalFontSize + delta, _fontSizeOptions.MinFontSize, _fontSizeOptions.MaxFontSize);
    }

}

