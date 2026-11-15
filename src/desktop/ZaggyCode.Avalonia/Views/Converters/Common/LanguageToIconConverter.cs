using System.Globalization;
using Material.Icons;

namespace ZaggyCode.Avalonia.Views.Converters.Common;

//#:NO_AI
public sealed class LanguageToIconConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Language language)
            return MaterialIconKind.Code;

        return language switch
        {
            Language.CSharp => MaterialIconKind.LanguageCsharp,
            Language.Python => MaterialIconKind.LanguagePython,
            _ => MaterialIconKind.Code
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
