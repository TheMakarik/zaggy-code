namespace ZaggyCode.Avalonia.ProxyImplementation;

public sealed class RobotGameTerminalProxy : IRobotGameTerminalProxy
{
    private const string ErrorColorPrefix = "Произошла ошибка: \x1b[91m";
    private const string ColorResetSuffix = "\x1b[0m";

    private readonly Lock _attachLock = new();
    private ScriptCommandLineSession? _session;

    public TextWriter Output => GetSession().Writer;

    public TextReader Input => GetSession().Reader;

    public void Attach(ScriptCommandLineSession session)
    {
        using var scope = _attachLock.EnterScope();
        _session = session;
    }

    public void WriteError(string text)
        => GetSession().Writer.WriteLine($"{ErrorColorPrefix}{text}{ColorResetSuffix}");

    private ScriptCommandLineSession GetSession()
    {
        using var scope = _attachLock.EnterScope();
        Debug.Assert(_session is not null, "Terminal proxy must be attached to a session before use");
        return _session!;
    }
}
