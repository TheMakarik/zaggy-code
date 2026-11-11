using System.Reflection;

namespace ZaggyCode.Avalonia.ViewModels.About;

public sealed partial class AboutViewModel : ViewModelBase
{
    private static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly() ?? typeof(AboutViewModel).Assembly;

    public string LogoPath { get; }
    public string ZaggyLovePath { get; }
    public string AppName { get; }
    public string Author { get; }
    public string Description { get; }
    public string Acknowledgements { get; }
    public bool UseOsDecoration { get; }
    public ObservableCollection<AboutComponentItem> Components { get; } = [];

    public Interaction<string, Unit> OpenUrlInteraction { get; } = new();

    public AboutViewModel(
        IOptions<AboutOptions> aboutOptions,
        IOptions<ZaggyAssetsOptions> zaggyAssets,
        IObservableStorage<ViewSettings> viewSettings,
        ILogger<AboutViewModel> logger)
    {
        var options = aboutOptions.Value;

        LogoPath = zaggyAssets.Value.LogoPath;
        ZaggyLovePath = zaggyAssets.Value.EmotionLove;
        AppName = EntryAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
            ?? EntryAssembly.GetName().Name
            ?? string.Empty;
        Author = EntryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;
        Description = options.ApplicationDescription;
        Acknowledgements = options.Acknowledgements;
        UseOsDecoration = viewSettings.Current.UseSystemTitleBar;

        foreach (var component in options.Components)
        {
            var iconPath = Path.Join(options.ComponentsIconsFolder, component.IconFileName);
            if (!File.Exists(iconPath))
            {
                logger.LogError("Не найдена иконка компонента «{name}»: {path}", component.Name, iconPath);
                continue;
            }

            Components.Add(new AboutComponentItem(
                component.Name,
                component.Version,
                component.Description,
                component.WebsiteUrl,
                component.GitHubUrl,
                iconPath));
        }
    }

    [ReactiveCommand]
    private async Task OpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        await OpenUrlInteraction.Handle(url);
    }
}
