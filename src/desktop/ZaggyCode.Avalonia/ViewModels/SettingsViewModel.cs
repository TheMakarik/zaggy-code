using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData.Binding;
using Material.Icons;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TextMateSharp.Grammars;
using ZaggyCode.Avalonia.Options;
using ZaggyCode.Avalonia.ViewModels.Messages;
using ZaggyCode.Core.Data.Interfaces;
using ZaggyCode.Modules.Data.Options;

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
    private readonly int _originalCodeFontSize;
    private readonly int _originalTerminalFontSize;
    private readonly bool _originalUseSystemTitleBar;
    private readonly bool _originalShowSidebar;
    private readonly string _originalCodeTheme;

    public int MinFontSize => _fontSizeOptions.MinFontSize;
    public int MaxFontSize => _fontSizeOptions.MaxFontSize;

    public ObservableCollection<CodeThemeItem> AvailableCodeThemes { get; } = [];
    public Interaction<Unit, Unit> CloseSettingsInteraction { get; } = new();
    public string CSharpExamplePath { get; }
    public string PythonExamplePath { get; }

    public SettingsViewModel(
        IUserStorage userStorage,
        IOptions<DefaultUser> defaultUserOptions,
        IOptions<CodeExamplePathOptions> codeExamplePathOptions,
        IOptions<FontSizeOptions> fontSizeOptions)
    {
        _userStorage = userStorage;
        _defaultUser = defaultUserOptions.Value;
        _fontSizeOptions = fontSizeOptions.Value;
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
    }

    private void InitializeAvailableCodeThemes()
    {
        foreach (ThemeName themeName in Enum.GetValues<ThemeName>())
        {
            AvailableCodeThemes.Add(new CodeThemeItem(
                themeName.ToString(),
                GetCodeThemeDisplayName(themeName),
                GetCodeThemeIconKind(themeName)));
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

    internal static string GetCodeThemeDisplayName(ThemeName themeName) => themeName switch
    {
        ThemeName.VisualStudioDark => "Visual Studio Dark",
        ThemeName.VisualStudioLight => "Visual Studio Light",
        ThemeName.Monokai => "Monokai",
        ThemeName.Dracula => "Dracula",
        ThemeName.SolarizedDark => "Solarized Dark",
        ThemeName.SolarizedLight => "Solarized Light",
        ThemeName.AtomOneDark => "Atom One Dark",
        ThemeName.AtomOneLight => "Atom One Light",
        ThemeName.Dark => "Dark",
        ThemeName.Light => "Light",
        ThemeName.DarkPlus => "Dark Plus",
        ThemeName.LightPlus => "Light Plus",
        ThemeName.Abbys => "Abyss",
        ThemeName.DimmedMonokai => "Dimmed Monokai",
        ThemeName.KimbieDark => "Kimbie Dark",
        ThemeName.QuietLight => "Quiet Light",
        ThemeName.Red => "Red",
        ThemeName.TomorrowNightBlue => "Tomorrow Night Blue",
        ThemeName.HighContrastLight => "High Contrast Light",
        ThemeName.HighContrastDark => "High Contrast Dark",
        _ => themeName.ToString()
    };

    internal static MaterialIconKind GetCodeThemeIconKind(ThemeName themeName) => themeName switch
    {
        ThemeName.VisualStudioDark or ThemeName.VisualStudioLight => MaterialIconKind.MicrosoftVisualStudio,
        ThemeName.LightPlus or ThemeName.DarkPlus => MaterialIconKind.Plus,
        ThemeName.HighContrastLight or ThemeName.HighContrastDark => MaterialIconKind.ContrastCircle,
        ThemeName.SolarizedDark or ThemeName.SolarizedLight => MaterialIconKind.WhiteBalanceSunny,
        ThemeName.AtomOneDark or ThemeName.AtomOneLight => MaterialIconKind.Atom,
        ThemeName.Dark => MaterialIconKind.WeatherNight,
        ThemeName.Light => MaterialIconKind.WhiteBalanceSunny,
        ThemeName.Dracula => MaterialIconKind.Blood,
        ThemeName.Monokai => MaterialIconKind.FruitCitrus,
        ThemeName.Abbys => MaterialIconKind.Water,
        ThemeName.DimmedMonokai => MaterialIconKind.Brightness2,
        ThemeName.KimbieDark => MaterialIconKind.Coffee,
        ThemeName.QuietLight => MaterialIconKind.VolumeMute,
        ThemeName.Red => MaterialIconKind.Palette,
        ThemeName.TomorrowNightBlue => MaterialIconKind.MoonWaningCrescent,
        _ => MaterialIconKind.Palette
    };
}

public sealed record CodeThemeItem(string ThemeName, string DisplayName, MaterialIconKind IconKind);
