namespace ZaggyCode.Avalonia.Views.Converters;

public sealed class ImagePathConverter : MarkupExtension, IValueConverter
{
    private static readonly ConcurrentDictionary<string, IImage?> Cache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
            return AvaloniaProperty.UnsetValue;

        return Cache.GetOrAdd(path, Load);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    private static IImage? Load(string path)
    {
        if (!File.Exists(path))
            return null;

        return Path.GetExtension(path).Equals(".svg", StringComparison.OrdinalIgnoreCase)
            ? new SvgImage { Source = SvgSource.Load(path) }
            : new Bitmap(path);
    }
}
