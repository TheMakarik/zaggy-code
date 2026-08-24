using System.Reflection;

namespace ZaggyCode.Avalonia.ViewModels;

public sealed class AboutViewModel(IOptions<ZaggyAssetsOptions> zaggyAssets) : ViewModelBase
{
    private static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly() ?? typeof(AboutViewModel).Assembly;

    public string LogoPath { get; } = zaggyAssets.Value.LogoPath;

    public string AppName { get; } = EntryAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
        ?? EntryAssembly.GetName().Name
        ?? string.Empty;

    public string Version { get; } = $"Версия: {EntryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+').First()
        ?? EntryAssembly.GetName().Version?.ToString()}";

    public string Description { get; } = EntryAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;

    public string Developers { get; } = EntryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;
}
