namespace ZaggyCode.Avalonia.Options;

public sealed class CodeExamplePathOptions
{
    public required string CSharpExamplePath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string PythonExamplePath
    {
        get => field;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
