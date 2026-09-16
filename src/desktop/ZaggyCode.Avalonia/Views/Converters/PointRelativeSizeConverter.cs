namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class PointRelativeSizeConverter : MarkupExtension, IMultiValueConverter
{
    private const double DefaultFraction = 0.25;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0 || values[0] is not IConvertible baseValue)
            return AvaloniaProperty.UnsetValue;

        var baseSize = System.Convert.ToDouble(baseValue, CultureInfo.InvariantCulture);

        return baseSize * ResolveFraction(values, parameter);
    }

    private static double ResolveFraction(IList<object?> values, object? parameter)
    {
        if (values.Count > 1 && values[1] is IConvertible fractionValue)
            return System.Convert.ToDouble(fractionValue, CultureInfo.InvariantCulture);

        return parameter switch
        {
            double value => value,
            string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => DefaultFraction
        };
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
