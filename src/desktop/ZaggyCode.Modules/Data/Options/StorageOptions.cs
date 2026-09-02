namespace ZaggyCode.Modules.Data.Options;

public sealed class StorageOptions
{
    public required string GameCodeDataPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string DataFilePath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required int WaitUserDataUpdateSeconds
    {
        get => field;
        set => field = value;
    }
    
    public required string PythonSettingsPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string CSharpSettingsPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
