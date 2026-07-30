namespace ZaggyCode.Modules.Archiving.Options;

public class TempOptions
{
    public required string TempDirectoryPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
