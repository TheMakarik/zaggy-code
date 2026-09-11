namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class FourDoublesToThicknessConverter : MarkupExtension, IMultiValueConverter
{
    public object? Convert(IList<object?>? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Debug.Assert(value is not null and {Count: 4});

        var left = ToDouble(value[0]);
        var top = ToDouble(value[1]);
        var right = ToDouble(value[2]);
        var bottom = ToDouble(value[3]);

        return new Thickness(left, top, right, bottom);
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    private static double ToDouble(object? v) => v switch
    {
        double d => d,
        int i => i,
        long l => l,
        IConvertible convertible => System.Convert.ToDouble(convertible, CultureInfo.InvariantCulture),
        _ => 0
    };
}
