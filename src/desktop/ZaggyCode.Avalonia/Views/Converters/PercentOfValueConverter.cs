namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class PercentOfValueConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IConvertible convertible)
            return 0d;

        var percent = parameter switch
        {
            double d => d,
            IConvertible c => System.Convert.ToDouble(c, culture),
            _ => 1d
        };

        return System.Convert.ToDouble(convertible, culture) * percent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
