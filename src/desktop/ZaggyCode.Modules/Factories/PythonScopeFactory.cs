namespace ZaggyCode.Modules.Factories;

public class PythonScopeFactory : IPythonScopeFactory
{
    private ScriptEngine _engine;
    private readonly ILogger<PythonScopeFactory> _logger;

    public PythonScopeFactory(ILogger<PythonScopeFactory> logger, IOptions<PythonScriptsOptions> options)
    {
        _logger = logger;
        _engine = Python.CreateEngine();
        _engine.SetSearchPaths([options.Value.StandardLibraryPath]);
        Debug.Assert(Directory.Exists(options.Value.StandardLibraryPath), $"{options.Value.StandardLibraryPath} must exists");
        logger.LogDebug("Created script engine for IronPython version {version}", _engine.LanguageVersion);
    }

    public ScriptScope GetFactory()
    {
        var result = _engine.CreateScope();
        Debug.Assert(result is not null);
        _logger.LogInformation("Created script scope for IronPython");
        return result;
    }
}