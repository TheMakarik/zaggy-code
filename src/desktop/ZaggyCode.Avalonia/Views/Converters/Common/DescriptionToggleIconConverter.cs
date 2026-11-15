using System.Globalization;
using Material.Icons;

namespace ZaggyCode.Avalonia.Views.Converters.Common;

//#:NO_AI
public sealed class DescriptionToggleIconConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isDescriptionVisible)
            return MaterialIconKind.BookOpenVariant;

        return isDescriptionVisible
            ? MaterialIconKind.CodeTags
            : MaterialIconKind.BookOpenVariant;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
