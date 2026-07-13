namespace ZaggyCode.Core.Languages.Lua;

[LanguageExtension(".lua")]
public sealed class LuaLanguageRunner(
    IUserStorage userStorage,
    ILogger<LuaLanguageRunner> logger,
    IOptions<SpeedMillisecondsOptions> speedOptions, 
    IOptions<LuaPathsOptions> luaOptions) : ILanguageRunner
{
    private readonly NLua.Lua _lua = new();
    private TextReader? _input;
    private TextWriter? _output;
    private ExecutionSpeed _speed;
    private IRobotExecutor? _executor;
    private CancellationToken _cancellationToken;
    
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public ILanguageRunner RedirectIo(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
        return this;
    }

    public ILanguageRunner SetSpeed(ExecutionSpeed speed)
    {
        _speed = speed;
        return this;
    }

    public ILanguageRunner SetExecutor(IRobotExecutor executor)
    {
        _executor = executor;
        return this;
    }

    public void Execute(string code, CancellationToken token)
    {
        _cancellationToken = token;
        
        logger.LogDebug("Preparing lua state to execute...");
        var stopwatch = Stopwatch.StartNew();
        
        var speedMilliseconds = _speed.GetActual(speedOptions.Value);
        _lua["__clr_wait"] = () => 
        { 
            var sleepChunksCount = speedMilliseconds / speedOptions.Value.SleepChunk;
            for (var i = 0; i < sleepChunksCount; i++)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(speedOptions.Value.SleepChunk);
                _cancellationToken.ThrowIfCancellationRequested();
            }
        };
        _lua["USE_TABLE_CONTENT_CHECKER"] = userStorage.Current.LuaData.UseTableContentCheckerByDefault;
        _lua["__clr_RobotExecutor"] = _executor;
        _lua["__clr_input"] = _input;
        _lua["__clr_output"] = _output;
        _lua["__debug"] = (string log) => { logger.LogDebug("Lua debug output: {log}", log); };
        _lua["__clr_DebugLineUpdated_raise"] = (int lineNumber) =>
        {
            _cancellationToken.ThrowIfCancellationRequested();
            DebugLineUpdated?.Invoke(this, new DebugLineUpdatedEventArgs() { LineNumber = lineNumber });
        };
        _lua["__clr_throws_IncorrectlyWroteNameException"] = (string actual, string best) =>
        {
            throw new LuaIncorrectlyWroteNameException(actual, best);
        };
        
        _lua.DoFile(luaOptions.Value.RegisterIoLuaPath);
        _lua.DoFile(luaOptions.Value.RegisterRobotLuaPath);
        _lua.DoFile(luaOptions.Value.RegisterIncorrectlyWroteNameCheckerLuaPath);
        _lua.DoFile(luaOptions.Value.SetNewLikeHookPath);
        _lua.State.Encoding = Encoding.UTF8;
       
        stopwatch.Stop();
        logger.LogDebug("Prepared lua state for {ms} ms", stopwatch.ElapsedMilliseconds);

        try
        {
            _lua.DoString(code);
        }
        catch (LuaScriptException exception) when (exception.InnerException is LuaIncorrectlyWroteNameException)
        {
            throw exception.InnerException;
        }
        catch (LuaScriptException exception) when (exception.InnerException is OperationCanceledException)
        {
            logger.LogDebug("Lua execution was cancelled");
        }
        catch (LuaScriptException exception)
        {
            logger.LogError(exception, "Lua exception: ");
            CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs()
            {
                Text = exception.Message
            });
        }
    }
    
    public void Dispose()
    {
        _lua.Dispose();
        logger.LogDebug("Lua state was disposed");
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}