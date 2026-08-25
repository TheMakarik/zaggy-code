namespace ZaggyCode.Modules.Theming.Options;

public class ThemeOptions
{
    public required string ThemeExtensions { get; set; }

    public required string ThemeFileName
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string SystemThemesFolder
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ExternThemesFolder
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
