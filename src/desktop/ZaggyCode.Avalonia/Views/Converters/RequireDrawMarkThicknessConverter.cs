namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class RequireDrawMarkThicknessConverter : MarkupExtension, IMultiValueConverter
{
    private const double DefaultThickness = 2d;
    private const double BoldMultiplier = 2d;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var thickness = values.Count > 0 && values[0] is IConvertible convertible
            ? System.Convert.ToDouble(convertible, CultureInfo.InvariantCulture)
            : DefaultThickness;

        var isBold = values.Count > 1 && values[1] is true;

        return isBold ? thickness * BoldMultiplier : thickness;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
