namespace ZaggyCode.Modules.Languages.Python;

//#:NO_AI
[LanguageExtension(".py")]
public sealed class PythonLanguageRunner : ILanguageRunner
{
    private const string NotSupportedText = "is not supported due to your application settings";
    
    // CLR variable names for Python interop
    private const string ClrOutput = "clr_output";
    private const string ClrInput = "clr_input";
    private const string ClrRaiseDebugLineUpdated = "clr_raise_debug_line_updated";
    private const string ClrRobotPath = "robot_path";
    private const string ClrRobotExecutorPrefix = "clr_RobotExecutor_";
    private const string ClrTryCancelExection = "clr_try_cancel_execution";

    private ScriptScope _python = IronPython.Hosting.Python.CreateEngine().CreateScope();
    private int _actualSpeed;
    private readonly ILogger<PythonLanguageRunner> _logger;
    private readonly IOptions<PythonScriptsOptions> _pythonOptions;
    private readonly IObservableStorage<PythonSettings> _pythonSettingsStorage;
    private readonly IOptions<SpeedMillisecondsOptions> _speedOptions;
    private readonly ILanguageSleepHelper _sleepHelper;

    public PythonLanguageRunner(ILogger<PythonLanguageRunner> logger,
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
        
        Debug.Assert(Directory.Exists(_pythonOptions.Value.StandardLibraryPath));
        _python.Engine.SetSearchPaths([pythonOptions.Value.StandardLibraryPath]);
    }

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public ILanguageRunner RedirectIo(TextReader input, TextWriter output)
    {
       _python.SetVariable(ClrOutput, (string text) =>
       {
           if (_pythonSettingsStorage.Current.SupressIo)
           {
               CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs(){Text = $"print() {NotSupportedText}"});
               return;
           }
           
           output.WriteLine(text);
           
       });
       _python.SetVariable(ClrInput, () =>
       {
           if (!_pythonSettingsStorage.Current.SupressIo) 
               return input.ReadLine();
           
           CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs(){Text = $"input() {NotSupportedText}"});
           return string.Empty;
       });

       _python.Engine.ExecuteFile(_pythonOptions.Value.RedirectIoPath, _python);
       _logger.LogInformation("Redirected IO for python (IO support = {ioSupport})", !_pythonSettingsStorage.Current.SupressIo);

       return this;
    }

    public ILanguageRunner SetSpeed(ExecutionSpeed speed)
    {
        _actualSpeed = speed.GetActual(_speedOptions.Value);
        _python.SetVariable(ClrRaiseDebugLineUpdated, (int line) =>
        {
            DebugLineUpdated?.Invoke(this, new DebugLineUpdatedEventArgs(){LineNumber = line});
        });
        _logger.LogInformation("Set python execution speed to {ms}ms", _actualSpeed);
        
        return this;
    }

    public ILanguageRunner SetExecutor(IRobotExecutor executor)
    {
        var methods = typeof(IRobotExecutor).GetMethods();
        _logger.LogDebug("Methods for executor: [{methods}]", string.Join(",", methods));
        SetExecutorVariables(methods, executor);
        
        _python.SetVariable(ClrRobotPath, _pythonOptions.Value.RobotPath);
        _python.Engine.ExecuteFile(_pythonOptions.Value.PrepareModules, _python);
        _logger.LogInformation("Set executor for python");
        return this;
    }

    public async Task Execute(string code, CancellationToken token)
    {
        try
        {
            Debug.Assert(Directory.Exists(_pythonOptions.Value.StandardLibraryPath));
            Debug.Assert(_python.ContainsVariable(ClrInput));
            Debug.Assert(_python.ContainsVariable(ClrOutput));
            Debug.Assert(_python.ContainsVariable(ClrRaiseDebugLineUpdated));
            Debug.Assert(_python.ContainsVariable(ClrRobotPath));
           
            await Task.Factory.StartNew(() =>
            {
                _python.SetVariable(ClrTryCancelExection, token.ThrowIfCancellationRequested);
                _python.Engine.ExecuteFile(_pythonOptions.Value.SetLineUpdatingPath, _python);

                DebugLineUpdated += (_, _) =>
                {
                    _sleepHelper.Sleep(
                        _actualSpeed,
                        _speedOptions.Value.SleepChunk,
                        token);
                };
                
                _python.Engine.CreateScriptSourceFromString(code, "<string>").Execute(_python);

                if (!_pythonSettingsStorage.Current.UseEntryFunction)
                    return;

                var main = _python.GetVariable(_pythonSettingsStorage.Current.EntryFunctionName);
                main();
                
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
        _python.Engine.ExecuteFile(_pythonOptions.Value.DisableLineUpdating, _python);
        _logger.LogInformation("Disposed Script Runner");
    }

    public async ValueTask DisposeAsync()
    {
        ClearEvents();
        await Task.Run(() =>
        {
            _python.Engine.ExecuteFile(_pythonOptions.Value.DisableLineUpdating, _python);
        });
        _logger.LogInformation("Disposed Script Runner");
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
    private string FormatVariable(string methodInfoName)
    {
        return ClrRobotExecutorPrefix + methodInfoName;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetExecutorVariables(MethodInfo[] methods, IRobotExecutor executor)
    {
        foreach (var methodInfo in methods)
            _python.SetVariable(
                FormatVariable(methodInfo.Name),
                () => methodInfo.Invoke(executor, null));
    }
}
