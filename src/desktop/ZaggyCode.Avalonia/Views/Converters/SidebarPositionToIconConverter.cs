namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class SidebarPositionToIconConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value?.ToString() switch
    {
        nameof(SidebarPosition.Left) => MaterialIconKind.PageLayoutSidebarLeft,
        nameof(SidebarPosition.Right) => MaterialIconKind.PageLayoutSidebarRight,
        nameof(SidebarPosition.Top) => MaterialIconKind.PageLayoutHeader,
        nameof(SidebarPosition.Hidden) => MaterialIconKind.EyeOff,
        _ => MaterialIconKind.PageLayoutSidebarLeft
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
