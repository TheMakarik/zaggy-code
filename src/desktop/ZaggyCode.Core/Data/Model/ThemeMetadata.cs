namespace ZaggyCode.Core.Data.Model;

public sealed class ThemeMetadata : ArchiveMetadata
{
    public required string BackgroundColor { get; set; }
    public required string PrimaryColor { get; set; }
    public required string ForegroundColor { get; set; }
    public required string ForegroundMutedColor { get; set; }
    public required string EditorBackgroundColor { get; set; }
    public required string EditorForegroundColor { get; set; }
    public required string SystemAccentColor { get; set; }
    
}