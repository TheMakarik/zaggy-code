namespace ZaggyCode.Avalonia.Options;

public sealed class AboutComponentOptions
{
    public required string Name { get; set; }

    public required string Version { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? WebsiteUrl { get; set; }

    public string? GitHubUrl { get; set; }

    public required string IconFileName { get; set; }
}
