namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class FalseToZeroOrTrueToParameterConverter  : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            false => 0d,
            true => (double)(parameter ?? throw new ArgumentNullException(nameof(parameter))),
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

}