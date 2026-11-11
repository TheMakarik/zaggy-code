namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class PythonValidationResultToStringConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PythonFunctionNameValidationResult result)
            return null;

        return result switch
        {
            PythonFunctionNameValidationResult.Success => string.Empty,
            PythonFunctionNameValidationResult.Empty => "Имя функции не может быть пустым",
            PythonFunctionNameValidationResult.ContainsSpaces => "Имя функции не должно содержать пробелов",
            PythonFunctionNameValidationResult.ContainsForbiddenCharacters => "Имя функции содержит запрещенные символы",
            PythonFunctionNameValidationResult.StartsWithDigit => "Имя функции не может начинаться с цифры",
            PythonFunctionNameValidationResult.StartsWithUnderscore => "Имя функции не может начинаться с подчеркивания",
            PythonFunctionNameValidationResult.IsReservedGlobalFunction => "Имя зарезервировано для глобальной функции",
            PythonFunctionNameValidationResult.IsStandardLibraryModule => "Имя совпадает с модулем стандартной библиотеки",
            PythonFunctionNameValidationResult.IsStandardLibraryFunction => "Имя совпадает с функцией стандартной библиотеки",
            _ => "Некорректное имя функции"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
