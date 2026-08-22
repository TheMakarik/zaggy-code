namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class BooleanToDynamicTextConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        Debug.Assert(values.Count == 3, "BooleanToDynamicTextConverter expects exactly 3 values: bool, trueText, falseText.");

        if (values.Count != 3)
            return string.Empty;

        if (values[0] is not bool flag)
            return string.Empty;

        var dynamicText = flag ? values[1]?.ToString() : values[2]?.ToString();
        var staticText = "боковую панель";

        return $"{dynamicText} {staticText}";
    }
}
