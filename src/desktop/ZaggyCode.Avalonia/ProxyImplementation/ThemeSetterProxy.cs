using ILogger = Serilog.ILogger;

namespace ZaggyCode.Avalonia.ProxyImplementation;

public sealed class ThemeSetterProxy(ILogger<ThemeSetterProxy> logger) : IThemeSetterProxy
{
    public void SetColor(string name, string hex)
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
            return;

        var color = Color.Parse(hex);
        logger.LogDebug("Установка цвета темы: {key}={value}", name, hex);
        resources[$"{name}Color"] = color;
        resources[$"{name}Brush"] = new SolidColorBrush(color);
    }
}
