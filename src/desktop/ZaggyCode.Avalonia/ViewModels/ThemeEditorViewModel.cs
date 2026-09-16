namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class ThemeEditorViewModel : ViewModelBase
{
    private readonly IThemeEditorService _editor;

    public ObservableCollection<ThemeColorEntry> Colors { get; } = [];

    public Interaction<bool, Unit> CloseInteraction { get; } = new();
    public Interaction<ColorPickerViewModel, Unit> OpenColorPicker { get; } = new();
    public Interaction<Unit, Unit> OpenAboutInteraction { get; } = new();
    public Interaction<Unit, Unit> OpenAboutAvaloniaInteraction { get; } = new();
    public Interaction<Unit, Unit> ViewXmlInteraction { get; } = new();
    public Interaction<Unit, Unit> EditMetadataInteraction { get; } = new();
    public Interaction<Unit, Unit> HelpInteraction { get; } = new();

    public string ThemeName => _editor.ThemeName ?? string.Empty;
    public bool IsDirty => _editor.IsDirty;

    public ThemeEditorViewModel(IThemeEditorService editor)
    {
        _editor = editor;

        _editor.PropertyChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(ThemeName));
            this.RaisePropertyChanged(nameof(IsDirty));
        };
    }

    public async Task InitializeAsync(string themeName)
    {
        await _editor.BeginEditAsync(themeName);

        Colors.Clear();
        foreach (var color in _editor.Colors)
            Colors.Add(color);

        this.RaisePropertyChanged(nameof(ThemeName));
        this.RaisePropertyChanged(nameof(IsDirty));
    }

    [ReactiveCommand]
    private async Task OpenColorPickerAsync(ThemeColorEntry entry)
    {
        var picker = new ColorPickerViewModel(entry.DisplayName, ParseColor(entry.Value));

        using var subscription = picker.WhenAnyValue(viewModel => viewModel.Color)
            .Skip(1)
            .Subscribe(color => _editor.SetColor(entry.PropertyName, ToHex(color)));

        await OpenColorPicker.Handle(picker);
    }

    [ReactiveCommand]
    private void ResetColor(ThemeColorEntry entry)
        => _editor.ResetColor(entry.PropertyName);

    [ReactiveCommand]
    private Task Save()
        => SaveAsync();

    [ReactiveCommand]
    private Task Cancel()
        => CancelAsync();

    public async Task SaveAsync()
    {
        await _editor.SaveAsync();
        await CloseInteraction.Handle(true);
    }

    public async Task CancelAsync()
    {
        _editor.Revert();
        await CloseInteraction.Handle(false);
    }

    [ReactiveCommand]
    private async Task ViewXmlAsync()
        => await ViewXmlInteraction.Handle(Unit.Default);

    [ReactiveCommand]
    private async Task EditMetadataAsync()
        => await EditMetadataInteraction.Handle(Unit.Default);

    [ReactiveCommand]
    private async Task OpenAboutAsync()
        => await OpenAboutInteraction.Handle(Unit.Default);

    [ReactiveCommand]
    private async Task OpenAboutAvaloniaAsync()
        => await OpenAboutAvaloniaInteraction.Handle(Unit.Default);

    [ReactiveCommand]
    private async Task OpenHelpAsync()
        => await HelpInteraction.Handle(Unit.Default);

    private static Color ParseColor(string hex)
        => Color.TryParse(hex, out var color) ? color : global::Avalonia.Media.Colors.White;

    private static string ToHex(Color color)
        => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
}
