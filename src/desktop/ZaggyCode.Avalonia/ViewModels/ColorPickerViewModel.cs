namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class ColorPickerViewModel(string colorName, Color color) : ViewModelBase
{
    [Reactive] private Color _color = color;

    public string ColorName { get; } = colorName;

    public Interaction<Unit, Unit> CloseInteraction { get; } = new();

    [ReactiveCommand]
    private async Task Close()
        => await CloseInteraction.Handle(Unit.Default);
}
