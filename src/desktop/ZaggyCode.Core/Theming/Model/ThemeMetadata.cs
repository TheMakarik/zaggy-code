namespace ZaggyCode.Core.Theming.Model;

public sealed class ThemeMetadata : ArchiveMetadata
{
    public required string Author { get; set; }
    public required string BackgroundColor { get; set; }
    public required string SidebarBackgroundColor { get; set; }
    public required string EditorBackgroundColor { get; set; }
    public required string TerminalBackgroundColor { get; set; }
    public required string PrimaryColor { get; set; }
    public required string BorderColor { get; set; }
    public required string ForegroundColor { get; set; }
}
