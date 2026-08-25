namespace ZaggyCode.Core.Theming.Model;
using System.Xml.Serialization;

[XmlRoot("theme")]
public sealed class Theme
{
    [XmlElement("background")]
    public required string BackgroundColor { get; set; }

    [XmlElement("background-secondary")]
    public required string BackgroundSecondaryColor { get; set; }

    [XmlElement("surface")]
    public required string SurfaceColor { get; set; }

    [XmlElement("surface-light")]
    public required string SurfaceLightColor { get; set; }

    [XmlElement("surface-hover")]
    public required string SurfaceHoverColor { get; set; }

    [XmlElement("primary")]
    public required string PrimaryColor { get; set; }

    [XmlElement("primary-light")]
    public required string PrimaryLightColor { get; set; }

    [XmlElement("primary-dark")]
    public required string PrimaryDarkColor { get; set; }

    [XmlElement("foreground")]
    public required string ForegroundColor { get; set; }

    [XmlElement("foreground-muted")]
    public required string ForegroundMutedColor { get; set; }

    [XmlElement("foreground-dark")]
    public required string ForegroundDarkColor { get; set; }

    [XmlElement("border")]
    public required string BorderColor { get; set; }

    [XmlElement("border-light")]
    public required string BorderLightColor { get; set; }

    [XmlElement("success")]
    public required string SuccessColor { get; set; }

    [XmlElement("success-dark")]
    public required string SuccessDarkColor { get; set; }

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

    [XmlElement("sidebar-background")]
    public required string SidebarBackgroundColor { get; set; }

    [XmlElement("accent")]
    public required string SystemAccentColor { get; set; }

    [XmlElement("accent-light1")]
    public required string SystemAccentColorLight1 { get; set; }

    [XmlElement("accent-dark1")]
    public required string SystemAccentColorDark1 { get; set; }

    [XmlElement("map-wall")]
    public required string MapWallColor { get; set; }

    [XmlElement("map-point-border")]
    public required string MapPointBorderColor { get; set; }

    [XmlElement("map-point-background")]
    public required string MapPointBackgroundColor { get; set; }
}
