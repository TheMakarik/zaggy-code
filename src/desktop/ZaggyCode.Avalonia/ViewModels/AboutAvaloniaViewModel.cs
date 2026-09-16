namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class AboutAvaloniaViewModel(IOptions<AboutAvaloniaOptions> options) : ViewModelBase
{
    public string LogoPath { get; } = options.Value.LogoPath;

    public string Title { get; } = options.Value.Title;

    public string Description { get; } = options.Value.Description;

    public string WebsiteText { get; } = options.Value.WebsiteText;

    public string WebsiteUrl { get; } = options.Value.WebsiteUrl;

    public string GitHubText { get; } = options.Value.GitHubText;

    public string GitHubUrl { get; } = options.Value.GitHubUrl;

    public Interaction<string, Unit> OpenUrlInteraction { get; } = new();

    [ReactiveCommand]
    private async Task OpenWebsite()
        => await OpenUrlInteraction.Handle(WebsiteUrl);

    [ReactiveCommand]
    private async Task OpenGitHub()
        => await OpenUrlInteraction.Handle(GitHubUrl);
}
