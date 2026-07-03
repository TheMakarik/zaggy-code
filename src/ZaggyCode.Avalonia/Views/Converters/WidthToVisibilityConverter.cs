using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Globalization;

namespace ZaggyCode.Avalonia.Views.Converters;

public class WidthToVisibilityConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double width)
            return false;

        var param = parameter?.ToString()?.Trim();
        if (string.IsNullOrEmpty(param))
            return true;

        // Expected formats: "<900", ">900", "<=900", ">=900"
        var op = string.Empty;
        var numberPart = param;

        if (param.StartsWith("<="))
        {
            op = "<=";
            numberPart = param[2..];
        }
        else if (param.StartsWith(">="))
        {
            op = ">=";
            numberPart = param[2..];
        }
        else if (param.StartsWith("<"))
        {
            op = "<";
            numberPart = param[1..];
        }
        else if (param.StartsWith(">"))
        {
            op = ">";
            numberPart = param[1..];
        }

        if (!double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var threshold))
            return true;

        return op switch
        {
            "<" => width < threshold,
            "<=" => width <= threshold,
            ">" => width > threshold,
            ">=" => width >= threshold,
            _ => true
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
