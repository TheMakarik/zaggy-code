namespace ZaggyCode.Modules.HostedServices.Options;

public sealed class LoggingCompressOptions
{
    public required int RetentionDays
    {
        get => field;
        set => field = value;
    }

    public required string LogsDirectoryPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ArchivesDirectoryPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
