namespace ZaggyCode.Modules.Archiving.Options;

public class TempOptions
{
    public required string TempDirectoryPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string TempToCompress
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string TempFromCompress
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
