namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class NullableDoubleConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IConvertible convertible)
            return AvaloniaProperty.UnsetValue;

        return System.Convert.ToDouble(convertible, culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
