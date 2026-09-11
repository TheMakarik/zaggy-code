namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class WallEdgeVisibilityConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not WallType wallType || parameter is not string edge)
            return false;

        return edge switch
        {
            "Full" => wallType == WallType.Full,
            "Top" => HasTop(wallType),
            "Bottom" => HasBottom(wallType),
            "Left" => HasLeft(wallType),
            "Right" => HasRight(wallType),
            _ => false
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    private static bool HasTop(WallType wall)
        => wall is WallType.Top or WallType.TopBottom or WallType.TopLeft or WallType.TopRight
            or WallType.TopBottomLeft or WallType.TopBottomRight or WallType.TopLeftRight or WallType.Full;

    private static bool HasBottom(WallType wall)
        => wall is WallType.Bottom or WallType.TopBottom or WallType.BottomLeft or WallType.BottomRight
            or WallType.TopBottomLeft or WallType.TopBottomRight or WallType.BottomLeftRight or WallType.Full;

    private static bool HasLeft(WallType wall)
        => wall is WallType.Left or WallType.TopLeft or WallType.BottomLeft or WallType.LeftRight
            or WallType.TopBottomLeft or WallType.TopLeftRight or WallType.BottomLeftRight or WallType.Full;

    private static bool HasRight(WallType wall)
        => wall is WallType.Right or WallType.TopRight or WallType.BottomRight or WallType.LeftRight
            or WallType.TopBottomRight or WallType.TopLeftRight or WallType.BottomLeftRight or WallType.Full;
}
