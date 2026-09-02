#if DEBUG
using Declarative.Avalonia.AgentTools;
#endif

namespace ZaggyCode.Avalonia;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            // Официальный DevTools MCP (avdt mcp) требует подписку Avalonia Plus:
            // на Community-тарифе сервер стартует, но любой вызов инструментов отвечает
            // "Your current subscription tier does not include access to this tool".
            // Поэтому для агентов используется бесплатный UseAgentInspector (loopback MCP на 127.0.0.1:5599).
            .WithDeveloperTools()
            .UseAgentInspector()
#endif
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI((_) => { });
    }
}