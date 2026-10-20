namespace ZaggyCode.Core.Theming.Model;
using System.Xml.Serialization;

[XmlRoot("theme")]
public sealed class Theme
{
    [XmlElement("background")]
    public required string WindowBackgroundColor { get; set; }

    [XmlElement("background-secondary")]
    public required string HeaderBackgroundColor { get; set; }

    [XmlElement("surface")]
    public required string CardBackgroundColor { get; set; }

    [XmlElement("surface-light")]
    public required string ElevatedBackgroundColor { get; set; }

    [XmlElement("surface-hover")]
    public required string HoverBackgroundColor { get; set; }

    [XmlElement("sidebar-background")]
    public required string SidebarBackgroundColor { get; set; }

    [XmlElement("primary")]
    public required string AccentColor { get; set; }

    [XmlElement("primary-light")]
    public required string AccentHoverColor { get; set; }

    [XmlElement("primary-dark")]
    public required string AccentPressedColor { get; set; }

    [XmlElement("accent")]
    public required string SystemAccentColor { get; set; }

    [XmlElement("foreground")]
    public required string TextPrimaryColor { get; set; }

    [XmlElement("foreground-muted")]
    public required string TextMutedColor { get; set; }

    [XmlElement("foreground-dark")]
    public required string TextSubtleColor { get; set; }

    [XmlElement("border")]
    public required string BorderColor { get; set; }

    [XmlElement("border-light")]
    public required string BorderHighlightColor { get; set; }

    [XmlElement("success")]
    public required string SuccessColor { get; set; }

    [XmlElement("success-dark")]
    public required string SuccessHoverColor { get; set; }

    [XmlElement("error")]
    public required string ErrorColor { get; set; }

    [XmlElement("warning")]
    public required string WarningColor { get; set; }

    [XmlElement("editor-background")]
    public required string EditorBackgroundColor { get; set; }

    [XmlElement("editor-foreground")]
    public required string EditorForegroundColor { get; set; }

    [XmlElement("editor-line-number")]
    public required string EditorLineNumberColor { get; set; }

    [XmlElement("terminal-background")]
    public required string TerminalBackgroundColor { get; set; }

    [XmlElement("terminal-foreground")]
    public required string TerminalForegroundColor { get; set; }

    [XmlElement("map-background")]
    public required string MapBackgroundColor { get; set; }

    [XmlElement("map-wall")]
    public required string MapWallColor { get; set; }

    [XmlElement("map-painted")]
    public required string MapPaintedCellColor { get; set; }

    [XmlElement("map-point-border")]
    public required string MapPointBorderColor { get; set; }

    [XmlElement("map-require-draw")]
    public required string MapRequireDrawColor { get; set; }

    [XmlElement("goal-color1")]
    public required string GoalColor1 { get; set; }

    [XmlElement("goal-color2")]
    public required string GoalColor2 { get; set; }
}
