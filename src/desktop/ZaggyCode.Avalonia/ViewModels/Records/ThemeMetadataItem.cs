namespace ZaggyCode.Avalonia.ViewModels.Records;

public sealed record ThemeMetadataItem(
    string Name,
    string Author,
    Version Version,
    string Description,
    IBrush Background,
    IBrush Sidebar,
    IBrush EditorBackground,
    IBrush TerminalBackground,
    IBrush Primary,
    IBrush Border,
    IBrush Foreground,
    bool IsSystemTheme)
{
    public static ThemeMetadataItem From(ThemeMetadata metadata) => new(
        metadata.Name,
        metadata.Author,
        metadata.CreatedAtVersion,
        metadata.Description ?? string.Empty,
        CreateBrush(metadata.BackgroundColor),
        CreateBrush(metadata.SidebarBackgroundColor),
        CreateBrush(metadata.EditorBackgroundColor),
        CreateBrush(metadata.TerminalBackgroundColor),
        CreateBrush(metadata.PrimaryColor),
        CreateBrush(metadata.BorderColor),
        CreateBrush(metadata.ForegroundColor),
        metadata.IsSystemTheme);

    private static IBrush CreateBrush(string hex)
        => new SolidColorBrush(Color.Parse(hex));
}
