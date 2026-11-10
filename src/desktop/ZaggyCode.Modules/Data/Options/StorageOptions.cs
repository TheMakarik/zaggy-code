namespace ZaggyCode.Modules.Data.Options;

public sealed class StorageOptions
{
    public required string GameCodeDataPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string StateSettingsPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ViewSettingsPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required int WaitSettingsUpdateSeconds
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

    public required string ThemeSettingsPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
