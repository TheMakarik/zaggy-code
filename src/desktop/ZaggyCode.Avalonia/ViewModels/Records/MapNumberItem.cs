namespace ZaggyCode.Avalonia.ViewModels.Records;

public sealed partial class MapNumberItem : ReactiveObject
{
    public int Number { get; }
    public MaterialIconKind Icon { get; }

    [Reactive] private bool _isSelected;

    public MapNumberItem(int number)
    {
        Number = number;
        Icon = Enum.TryParse<MaterialIconKind>($"Numeric{number}", out var kind)
            ? kind
            : MaterialIconKind.Numeric;
    }
}
