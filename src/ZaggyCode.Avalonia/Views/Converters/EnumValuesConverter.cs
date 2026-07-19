namespace ZaggyCode.Avalonia.Views.Converters;

public class EnumValuesConverter : MarkupExtension, IValueConverter
{
    public Type? EnumType { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Type? type = value as Type ?? EnumType;
        if (type is null || !type.IsEnum)
            return Array.Empty<object>();
        
        return type.GetFields()
            .Where(f => !f.IsSpecialName) //скрыть value__ - настоящие чисдовое значение у System.Enum
            .Select(f =>
        {

            LanguagePrettyNameAttribute? attribute = f.GetCustomAttribute<LanguagePrettyNameAttribute>();
            return attribute is not null 
                ? attribute.Name
                : f.Name;
        });
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
