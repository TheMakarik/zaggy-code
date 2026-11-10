namespace ZaggyCode.Avalonia.ViewModels.Records;

public sealed record AboutComponentItem(
    string Name,
    string Version,
    string Description,
    string? WebsiteUrl,
    string? GitHubUrl,
    string IconPath)
{
    public bool HasWebsite => !string.IsNullOrWhiteSpace(WebsiteUrl);
    public bool HasGitHub => !string.IsNullOrWhiteSpace(GitHubUrl);
}
