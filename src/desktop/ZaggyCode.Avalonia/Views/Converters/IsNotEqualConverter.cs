namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class IsNotEqualConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
            return false;

        return !string.Equals(values[0]?.ToString(), values[1]?.ToString(), StringComparison.Ordinal);
    }
}
