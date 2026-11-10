namespace ZaggyCode.Avalonia.Options;

public sealed class AboutOptions
{
    public string ApplicationDescription { get; set; } = string.Empty;

    public string Acknowledgements { get; set; } = string.Empty;

    public AboutComponentOptions[] Components { get; set; } = [];

    public required string ComponentsIconsFolder
    {
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
