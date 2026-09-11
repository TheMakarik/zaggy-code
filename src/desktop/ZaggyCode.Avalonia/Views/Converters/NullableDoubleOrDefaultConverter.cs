namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class NullableDoubleOrDefaultConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IConvertible convertible)
            return System.Convert.ToDouble(convertible, culture);

        if (parameter is IConvertible fallback)
            return System.Convert.ToDouble(fallback, culture);

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
