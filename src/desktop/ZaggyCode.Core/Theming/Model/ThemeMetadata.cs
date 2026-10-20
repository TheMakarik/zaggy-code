using System.Text.Json.Serialization;

namespace ZaggyCode.Core.Theming.Model;

public sealed class ThemeMetadata : ArchiveMetadata
{
    [JsonPropertyName("author")]
    public required string Author { get; set; }

    [JsonPropertyName("background-color")]
    public required string WindowBackgroundColor { get; set; }

    [JsonPropertyName("sidebar-background-color")]
    public required string SidebarBackgroundColor { get; set; }

    [JsonPropertyName("editor-background-color")]
    public required string EditorBackgroundColor { get; set; }

    [JsonPropertyName("terminal-background-color")]
    public required string TerminalBackgroundColor { get; set; }

    [JsonPropertyName("primary-color")]
    public required string AccentColor { get; set; }

    [JsonPropertyName("border-color")]
    public required string BorderColor { get; set; }

    [JsonPropertyName("foreground-color")]
    public required string TextPrimaryColor { get; set; }

    [JsonIgnore]
    public bool IsSystemTheme { get; set; }

    [JsonIgnore]
    public string? Path { get; set; }
}
