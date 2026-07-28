namespace ZaggyCode.Core.Data.Model;

public sealed class Theme
{
    public required string BackgroundColor { get; set; }
    public required string BackgroundSecondaryColor { get; set; }
    public required string SurfaceColor { get; set; }
    public required string SurfaceLightColor { get; set; }
    public required string SurfaceHoverColor { get; set; }

    public required string PrimaryColor { get; set; }
    public required string PrimaryLightColor { get; set; }
    public required string PrimaryDarkColor { get; set; }

    public required string ForegroundColor { get; set; }
    public required string ForegroundMutedColor { get; set; }
    public required string ForegroundDarkColor { get; set; }

    public required string BorderColor { get; set; }
    public required string BorderLightColor { get; set; }

    public required string SuccessColor { get; set; }
    public required string SuccessDarkColor { get; set; }
    public required string ErrorColor { get; set; }
    public required string WarningColor { get; set; }

    public required string EditorBackgroundColor { get; set; }
    public required string EditorForegroundColor { get; set; }
    public required string EditorLineNumberColor { get; set; }

    public required string TerminalBackgroundColor { get; set; }
    public required string TerminalForegroundColor { get; set; }

    public required string SidebarBackgroundColor { get; set; }

    public required string SystemAccentColor { get; set; }
    public required string SystemAccentColorLight1 { get; set; }
    public required string SystemAccentColorDark1 { get; set; }
}
