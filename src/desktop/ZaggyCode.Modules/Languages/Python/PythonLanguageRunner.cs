namespace ZaggyCode.Modules.Languages.Python;

[LanguageExtension(".py")]
public sealed class PythonLanguageRunner : ILanguageRunner
{
    private const string NotSupportedText = "is not supported due to your application settings";

    // Источник пользовательского кода задаётся в Execute через CreateScriptSourceFromString(code, UserCodeFileName);
    // фреймыIronPython importlib тоже имеют co_filename "<string>", поэтому фильтруем ещё и по co_name.
    private const string UserCodeFileName = "<string>";
    private const string ModuleName = "<module>";
    private const string LineUpdateTraceEventName = "line";

    private const string ClrOutput = "clr_output";
    private const string ClrInput = "clr_input";
    private const string ClrRobotPath = "robot_path";
    private const string ClrRobotExecutorPrefix = "clr_RobotExecutor_";

    private readonly ScriptScope _python;
    private int _actualSpeed;
    private readonly ILogger<PythonLanguageRunner> _logger;
    private readonly IOptions<PythonScriptsOptions> _pythonOptions;
    private readonly IObservableStorage<PythonSettings> _pythonSettingsStorage;
    private readonly IOptions<SpeedMillisecondsOptions> _speedOptions;
    private readonly ILanguageSleepHelper _sleepHelper;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public PythonLanguageRunner(
        ILogger<PythonLanguageRunner> logger,
        IOptions<PythonScriptsOptions> pythonOptions,
        IObservableStorage<PythonSettings> pythonSettingsStorage,
        IOptions<SpeedMillisecondsOptions> speedOptions,
        ILanguageSleepHelper sleepHelper)
    {
        _logger = logger;
        _pythonOptions = pythonOptions;
        _pythonSettingsStorage = pythonSettingsStorage;
        _speedOptions = speedOptions;
        _sleepHelper = sleepHelper;

        var engine = IronPython.Hosting.Python.CreateEngine();
        _python = engine.CreateScope();

        Debug.Assert(Directory.Exists(_pythonOptions.Value.StandardLibraryPath));
        engine.SetSearchPaths([_pythonOptions.Value.StandardLibraryPath]);
    }

    public void RedirectIo(TextReader input, TextWriter output, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _python.Engine.SetTrace(CreateCancellationTrace(token));

        _python.SetVariable(ClrOutput, CreateOutputHandler(output));
        _python.SetVariable(ClrInput, CreateInputHandler(input));
        _python.Engine.ExecuteFile(_pythonOptions.Value.RedirectIoPath, _python);

        _logger.LogInformation("Redirected IO for python (IO support = {ioSupport})", !_pythonSettingsStorage.Current.SupressIo);
    }

    public void SetSpeed(ExecutionSpeed speed, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _python.Engine.SetTrace(CreateCancellationTrace(token));

        _actualSpeed = speed.GetActual(_speedOptions.Value);

        _logger.LogInformation("Set python execution speed to {ms}ms", _actualSpeed);
    }

    public void SetExecutor(IRobotExecutor executor, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _python.Engine.SetTrace(CreateCancellationTrace(token));

        var methods = typeof(IRobotExecutor).GetMethods();
        _logger.LogDebug("Methods for executor: [{methods}]", string.Join(",", methods));

        SetExecutorVariables(methods, executor);
        _python.SetVariable(ClrRobotPath, _pythonOptions.Value.RobotPath);
        _python.Engine.ExecuteFile(_pythonOptions.Value.PrepareModules, _python);

        _logger.LogInformation("Set executor for python");
    }

    public async Task Execute(string code, CancellationToken token)
    {
        try
        {
            ValidatePythonEnvironment();

            await Task.Factory.StartNew(() =>
            {
                _python.Engine.SetTrace(CreateUserCodeTrace(token));

                void OnDebugLineUpdated(object? sender, DebugLineUpdatedEventArgs args) =>
                    _sleepHelper.Sleep(_actualSpeed, _speedOptions.Value.SleepChunk, token);

                DebugLineUpdated += OnDebugLineUpdated;
                try
                {
                    _python.Engine.CreateScriptSourceFromString(code, UserCodeFileName).Execute(_python);

                    if (!_pythonSettingsStorage.Current.UseEntryFunction)
                        return;

                    var main = _python.GetVariable(_pythonSettingsStorage.Current.EntryFunctionName);
                    main();
                }
                finally
                {
                    DebugLineUpdated -= OnDebugLineUpdated;
                }
            }, TaskCreationOptions.LongRunning);
        }
        catch (Exception e) when (e is OperationCanceledException || e.InnerException is OperationCanceledException)
        {
            _logger.LogDebug("Code execution was canceled");
        }
        catch (Exception e) when (IsPythonException(e))
        {
            CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs { Text = $"{e.GetType().Name} {e.Message}".Trim() });
            _logger.LogWarning(e, "Python execution error: {ErrorType}", e.GetType().Name);
        }
        catch (Exception e)
        {
            CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs { Text = $".NET error: {e}" });
            _logger.LogError(e, "Unexpected error during Python execution");
        }
    }

    public void Dispose()
    {
        ClearEvents();
        _python.Engine.SetTrace(null);
        _logger.LogInformation("Disposed Script Runner");
    }

    public ValueTask DisposeAsync()
    {
        ClearEvents();
        _python.Engine.SetTrace(null);
        _logger.LogInformation("Disposed Script Runner");

        return ValueTask.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearEvents()
    {
        DebugLineUpdated = null;
        CodeErrorOccurred = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsPythonException(Exception e)
    {
        var @namespace = e.GetType().Namespace ?? string.Empty;
        return @namespace.Contains(nameof(IronPython)) || @namespace.Contains(nameof(Microsoft.Scripting));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string FormatVariable(string methodInfoName) => ClrRobotExecutorPrefix + methodInfoName;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetExecutorVariables(MethodInfo[] methods, IRobotExecutor executor)
    {
        foreach (var methodInfo in methods)
        {
            _python.SetVariable(
                FormatVariable(methodInfo.Name),
                () => methodInfo.Invoke(executor, null));
        }
    }

    private TracebackDelegate CreateUserCodeTrace(CancellationToken token)
    {
        TracebackDelegate OnUserCodeLine(TraceBackFrame frame, string traceEvent, object payload)
        {
            token.ThrowIfCancellationRequested();

            if (traceEvent == LineUpdateTraceEventName && frame.f_code.co_filename == UserCodeFileName && frame.f_code.co_name == ModuleName)
                DebugLineUpdated?.Invoke(this, new DebugLineUpdatedEventArgs { LineNumber = Convert.ToInt32(frame.f_lineno) });

            return OnUserCodeLine;
        }

        return OnUserCodeLine;
    }

    // Трейс для фоновых скриптов настройки: на каждой строке только проверяет токен,
    // чтобы отменённый сетап прерывался посреди ExecuteFile, но не поднимал DebugLineUpdated.
    private TracebackDelegate CreateCancellationTrace(CancellationToken token)
    {
        TracebackDelegate OnSetupLine(TraceBackFrame frame, string traceEvent, object payload)
        {
            token.ThrowIfCancellationRequested();

            return OnSetupLine;
        }

        return OnSetupLine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Action<string> CreateOutputHandler(TextWriter output)
    {
        return text =>
        {
            if (_pythonSettingsStorage.Current.SupressIo)
            {
                CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs { Text = $"print() {NotSupportedText}" });
                return;
            }

            output.WriteLine(text);
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Func<string> CreateInputHandler(TextReader input)
    {
        return () =>
        {
            if (_pythonSettingsStorage.Current.SupressIo)
            {
                CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs { Text = $"input() {NotSupportedText}" });
                return string.Empty;
            }

            return input.ReadLine();
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidatePythonEnvironment()
    {
        Debug.Assert(Directory.Exists(_pythonOptions.Value.StandardLibraryPath));
        Debug.Assert(_python.ContainsVariable(ClrInput));
        Debug.Assert(_python.ContainsVariable(ClrOutput));
        Debug.Assert(_python.ContainsVariable(ClrRobotPath));
    }
}
