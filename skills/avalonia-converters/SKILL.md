
# Создание конвертеров

## Правильный подход
Конвертеры должны быть реализованы как **MarkupExtension**, чтобы использовать их в XAML без создания экземпляров в ресурсах.

## Базовая структура

```csharp
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

public sealed class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return AvaloniaProperty.UnsetValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

## Когда ConvertBack не поддерживается
Всегда выбрасывай `NotSupportedException`:

```csharp
public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
{
    throw new NotSupportedException();
}
```

## Пример с enum

```csharp
public sealed class PythonValidationResultToStringConverter : MarkupExtension, IValueConverter
{
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

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

## Неправильный подход (не использовать)

```csharp
// ПЛОХО — без MarkupExtension
public sealed class MyConverter : IValueConverter
{
    // Регистрируется в ресурсах, требует создания экземпляра
}
```

## Именование
- Название конвертера должно отражать что он делает: `BooleanToVisibilityConverter`, `EnumToStringConverter`
- Суффикс `Converter` обязателен
- Размещай в папке `Views/Converters/`

## MultiBinding конвертеры
```csharp
public sealed class IsNotEqualConverter : MarkupExtension, IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
            return false;

        return !Equals(values[0], values[1]);
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```
