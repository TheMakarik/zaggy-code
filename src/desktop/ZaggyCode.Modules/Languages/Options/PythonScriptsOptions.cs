namespace ZaggyCode.Modules.Languages.Options;

public class PythonScriptsOptions
{
    public required string EnableRobotPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string RedirectIoPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string SetLineUpdatingPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string RobotPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
    
    public required string StandardLibraryPath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}