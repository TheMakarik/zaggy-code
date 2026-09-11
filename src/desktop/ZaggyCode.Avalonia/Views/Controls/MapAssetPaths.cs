namespace ZaggyCode.Avalonia.Views.Controls;

public sealed class MapAssetPaths
{
    public static readonly AttachedProperty<string?> CoinPathProperty =
        AvaloniaProperty.RegisterAttached<MapAssetPaths, Control, string?>("CoinPath");

    public static string? GetCoinPath(Control element) => element.GetValue(CoinPathProperty);

    public static void SetCoinPath(Control element, string? value) => element.SetValue(CoinPathProperty, value);
}
