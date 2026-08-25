using System.Globalization;
using System.Collections;

namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class IsEqualConverter : MarkupExtension, IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
            return false;

        return Equals(values[0], values[1]);
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
