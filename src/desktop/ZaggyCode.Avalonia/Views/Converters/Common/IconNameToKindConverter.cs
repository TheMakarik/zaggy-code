using System.Globalization;
using Material.Icons;

namespace ZaggyCode.Avalonia.Views.Converters.Common;

//#:NO_AI
public sealed class IconNameToKindConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string iconName)
            return MaterialIconKind.Palette;

        return Enum.TryParse<MaterialIconKind>(iconName, out var kind)
            ? kind
            : MaterialIconKind.Palette;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
