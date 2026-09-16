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
            string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            IConvertible c => System.Convert.ToDouble(c, CultureInfo.InvariantCulture),
            _ => 1d
        };

        return System.Convert.ToDouble(convertible, CultureInfo.InvariantCulture) * percent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
