namespace ZaggyCode.Avalonia.Options;

public sealed class AboutAvaloniaOptions
{
    public required string LogoPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string WebsiteText { get; set; }

    public required string WebsiteUrl { get; set; }

    public required string GitHubText { get; set; }

    public required string GitHubUrl { get; set; }
}
