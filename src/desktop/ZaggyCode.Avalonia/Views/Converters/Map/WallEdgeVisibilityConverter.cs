namespace ZaggyCode.Avalonia.Views.Converters.Map;

public sealed class WallEdgeVisibilityConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not RobotWallType wallType || parameter is not string edge)
            return false;

        return edge switch
        {
            "Full" => wallType == RobotWallType.Full,
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

    private static bool HasTop(RobotWallType robotWall)
        => robotWall is RobotWallType.Top or RobotWallType.TopBottom or RobotWallType.TopLeft or RobotWallType.TopRight
            or RobotWallType.TopBottomLeft or RobotWallType.TopBottomRight or RobotWallType.TopLeftRight or RobotWallType.Full;

    private static bool HasBottom(RobotWallType robotWall)
        => robotWall is RobotWallType.Bottom or RobotWallType.TopBottom or RobotWallType.BottomLeft or RobotWallType.BottomRight
            or RobotWallType.TopBottomLeft or RobotWallType.TopBottomRight or RobotWallType.BottomLeftRight or RobotWallType.Full;

    private static bool HasLeft(RobotWallType robotWall)
        => robotWall is RobotWallType.Left or RobotWallType.TopLeft or RobotWallType.BottomLeft or RobotWallType.LeftRight
            or RobotWallType.TopBottomLeft or RobotWallType.TopLeftRight or RobotWallType.BottomLeftRight or RobotWallType.Full;

    private static bool HasRight(RobotWallType robotWall)
        => robotWall is RobotWallType.Right or RobotWallType.TopRight or RobotWallType.BottomRight or RobotWallType.LeftRight
            or RobotWallType.TopBottomRight or RobotWallType.TopLeftRight or RobotWallType.BottomLeftRight or RobotWallType.Full;
}
