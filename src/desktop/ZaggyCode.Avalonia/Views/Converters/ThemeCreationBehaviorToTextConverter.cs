namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class ThemeCreationBehaviorToTextConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        ThemeCreationBehavior.OpenEditorAndApply => "Открыть редактор и применить",
        ThemeCreationBehavior.ApplyOnly => "Просто применить",
        ThemeCreationBehavior.Nothing => "Ничего не делать",
        _ => string.Empty
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
