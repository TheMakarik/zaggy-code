using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Globalization;

namespace ZaggyCode.Avalonia.Views.Converters;

public class EnumValuesConverter : MarkupExtension, IValueConverter
{
    public Type? EnumType { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var type = value as Type ?? EnumType;
        if (type is null || !type.IsEnum)
            return Array.Empty<object>();

        return Enum.GetValues(type);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
