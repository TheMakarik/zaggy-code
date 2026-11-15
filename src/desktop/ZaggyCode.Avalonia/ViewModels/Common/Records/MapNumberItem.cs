namespace ZaggyCode.Avalonia.ViewModels.Common.Records;

public sealed partial class MapNumberItem(int number) : ReactiveObject
{
    public int Number { get; } = number;

    [Reactive] private bool _isSelected;
}
