namespace ZaggyCode.Modules.Archiving.Options;

public class MetadataOptions
{
    public required string MetadataFile
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
