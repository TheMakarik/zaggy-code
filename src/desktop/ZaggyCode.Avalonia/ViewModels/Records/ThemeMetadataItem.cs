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
    bool IsSystemTheme,
    ThemeMetadata Source)
{
    public ICommand? CopyCommand { get; init; }
    public ICommand? DeleteCommand { get; init; }
    public ICommand? EditCommand { get; init; }

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
        metadata.IsSystemTheme,
        metadata);

    private static IBrush CreateBrush(string hex)
        => new SolidColorBrush(Color.Parse(hex));
}
