using Material.Icons.Avalonia;

namespace ZaggyCode.Avalonia.Views.Converters.Themes;

public sealed class ThemeCreationBehaviorToIconConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        ThemeCreationBehavior.OpenEditorAndApply => MaterialIconKind.Pencil,
        ThemeCreationBehavior.ApplyOnly => MaterialIconKind.Check,
        ThemeCreationBehavior.Nothing => MaterialIconKind.Minus,
        _ => MaterialIconKind.HelpCircle
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
