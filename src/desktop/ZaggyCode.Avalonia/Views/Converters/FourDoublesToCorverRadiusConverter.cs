using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ZaggyCode.Avalonia.Views.Converters;

public class FourDoublesToCorverRadiusConverter : MarkupExtension, IMultiValueConverter
{
    public object? Convert(IList<object?>? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Debug.Assert(value is not null and {Count: 4});

        var leftTop = ToDouble(value[0]);
        var leftBottom = ToDouble(value[1]);
        var rightTop = ToDouble(value[2]);
        var rightBottom = ToDouble(value[3]);

        return new CornerRadius(leftTop, rightTop, rightBottom, leftBottom);
    }

    private static double ToDouble(object? value) => value switch
    {
        double d => d,
        int i => i,
        long l => l,
        _ when value is IConvertible convertible => System.Convert.ToDouble(convertible, CultureInfo.InvariantCulture),
        _ => 0
    };

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}