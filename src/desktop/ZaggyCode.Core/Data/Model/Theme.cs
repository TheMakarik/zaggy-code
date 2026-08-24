namespace ZaggyCode.Core.Data.Model;
using System.Xml.Serialization;

[XmlRoot("theme")]
public sealed class Theme
{
    [XmlElement("background:color")]
    public required string BackgroundColor { get; set; }

    [XmlElement("background:secondary-color")]
    public required string BackgroundSecondaryColor { get; set; }

    [XmlElement("surface:color")]
    public required string SurfaceColor { get; set; }

    [XmlElement("surface:light-color")]
    public required string SurfaceLightColor { get; set; }

    [XmlElement("surface:hover-color")]
    public required string SurfaceHoverColor { get; set; }

    [XmlElement("primary:color")]
    public required string PrimaryColor { get; set; }

    [XmlElement("primary:light-color")]
    public required string PrimaryLightColor { get; set; }

    [XmlElement("primary:dark-color")]
    public required string PrimaryDarkColor { get; set; }

    [XmlElement("foreground:color")]
    public required string ForegroundColor { get; set; }

    [XmlElement("foreground:muted-color")]
    public required string ForegroundMutedColor { get; set; }

    [XmlElement("foreground:dark-color")]
    public required string ForegroundDarkColor { get; set; }

    [XmlElement("border:color")]
    public required string BorderColor { get; set; }

    [XmlElement("border:light-color")]
    public required string BorderLightColor { get; set; }

    [XmlElement("success:color")]
    public required string SuccessColor { get; set; }

    [XmlElement("success:dark-color")]
    public required string SuccessDarkColor { get; set; }

    [XmlElement("error:color")]
    public required string ErrorColor { get; set; }

    [XmlElement("warning:color")]
    public required string WarningColor { get; set; }

    [XmlElement("editor:background-color")]
    public required string EditorBackgroundColor { get; set; }

    [XmlElement("editor:foreground-color")]
    public required string EditorForegroundColor { get; set; }

    [XmlElement("editor:line-number-color")]
    public required string EditorLineNumberColor { get; set; }

    [XmlElement("terminal:background-color")]
    public required string TerminalBackgroundColor { get; set; }

    [XmlElement("terminal:foreground-color")]
    public required string TerminalForegroundColor { get; set; }

    [XmlElement("sidebar:background-color")]
    public required string SidebarBackgroundColor { get; set; }

    [XmlElement("system:accent-color")]
    public required string SystemAccentColor { get; set; }

    [XmlElement("system:accent-color-light1")]
    public required string SystemAccentColorLight1 { get; set; }

    [XmlElement("system:accent-color-dark1")]
    public required string SystemAccentColorDark1 { get; set; }
}