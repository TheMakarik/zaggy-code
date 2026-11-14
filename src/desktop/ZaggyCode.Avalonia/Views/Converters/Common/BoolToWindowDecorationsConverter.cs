namespace ZaggyCode.Avalonia.Views.Converters.Common;

public class BoolToWindowDecorationsConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return WindowDecorations.Full;

        return WindowDecorations.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowDecorations decorations)
            return decorations == WindowDecorations.Full;

        return false;
    }
}
