namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class SidebarPositionToTextConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value?.ToString() switch
    {
        nameof(SidebarPosition.Left) => "Слева",
        nameof(SidebarPosition.Right) => "Справа",
        nameof(SidebarPosition.Top) => "Сверху",
        nameof(SidebarPosition.Hidden) => "Не показывать",
        _ => string.Empty
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
