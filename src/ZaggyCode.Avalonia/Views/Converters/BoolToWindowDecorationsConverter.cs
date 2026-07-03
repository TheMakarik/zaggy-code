using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Globalization;
using Material.Icons;

namespace ZaggyCode.Avalonia.Views.Converters;

public class BoolToWindowDecorationsConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return WindowDecorations.Full;

        return WindowDecorations.BorderOnly;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowDecorations decorations)
            return decorations == WindowDecorations.Full;

        return false;
    }
}
