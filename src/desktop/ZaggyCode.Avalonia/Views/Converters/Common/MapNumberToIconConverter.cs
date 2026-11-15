using System.Globalization;
using Material.Icons;

namespace ZaggyCode.Avalonia.Views.Converters.Common;

//#:NO_AI
public sealed class MapNumberToIconConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int number)
            return MaterialIconKind.Numeric;

        return Enum.TryParse<MaterialIconKind>($"Numeric{number}", out var kind)
            ? kind
            : MaterialIconKind.Numeric;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
