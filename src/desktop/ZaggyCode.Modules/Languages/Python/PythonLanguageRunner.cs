using System.Runtime.CompilerServices;

namespace ZaggyCode.Modules.Languages.Python;

[LanguageExtension(".py")]
public sealed class PythonLanguageRunner(
    ILogger<PythonLanguageRunner> logger,
    IOptions<PythonScriptsOptions> pythonOptions,
    IPythonSettingsStorage pythonSettingsStorage,
    IOptions<SpeedMillisecondsOptions> speedOptions,
    IPythonScopeFactory pythonScopeFactory,
    ILanguageSleepHelper sleepHelper)
    : ILanguageRunner
{
    private const string NotSupportedText = "is not supported due to your application settings";
    
    // CLR variable names for Python interop
    private const string ClrOutput = "clr_output";
    private const string ClrInput = "clr_input";
    private const string ClrRaiseDebugLineUpdated = "clr_raise_debug_line_updated";
    private const string ClrRobotPath = "robot_path";
    private const string ClrRobotExecutorPrefix = "clr_RobotExecutor_";
    private const string ClrTryCancelExection = "clr_try_cancel_execution";
    
    private ScriptScope _python = pythonScopeFactory.GetFactory();
    private int _actualSpeed;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public ILanguageRunner RedirectIo(TextReader input, TextWriter output)
    {
       _python.SetVariable(ClrOutput, (string text) =>
       {
           if (pythonSettingsStorage.Current.SupressIo)
           {
               CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs(){Text = $"print() {NotSupportedText}"});
               return;
           }
           
           output.WriteLine(text);
           
       });
       _python.SetVariable(ClrInput, () =>
       {
           if (!pythonSettingsStorage.Current.SupressIo) 
               return input.ReadLine();
           
           CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs(){Text = $"input() {NotSupportedText}"});
           return string.Empty;
       });

       _python.Engine.ExecuteFile(pythonOptions.Value.RedirectIoPath, _python);
       logger.LogInformation("Redirected IO for python (IO support = {ioSupport}", !pythonSettingsStorage.Current.SupressIo);

       return this;
    }

    public ILanguageRunner SetSpeed(ExecutionSpeed speed)
    {
        _actualSpeed = speed.GetActual(speedOptions.Value);
        _python.SetVariable(ClrRaiseDebugLineUpdated, (int line) =>
        {
            DebugLineUpdated?.Invoke(this, new DebugLineUpdatedEventArgs(){LineNumber = line});
        });
        logger.LogInformation("Set python execution speed to {ms}ms", _actualSpeed);
        
        return this;
    }

    public ILanguageRunner SetExecutor(IRobotExecutor executor)
    {
        var methods = typeof(IRobotExecutor).GetMethods();
        logger.LogDebug("Methods for executor: [{methods}]", string.Join(",", methods));
        SetExecutorVariables(methods, executor);
        
        _python.SetVariable(ClrRobotPath, pythonOptions.Value.RobotPath);
        _python.Engine.ExecuteFile(pythonOptions.Value.EnableRobotPath, _python);
        logger.LogInformation("Set executor for python");
        return this;
    }

    public async Task Execute(string code, CancellationToken token)
    {
        try
        {
            Debug.Assert(_python.ContainsVariable(ClrInput));
            Debug.Assert(_python.ContainsVariable(ClrOutput));
            Debug.Assert(_python.ContainsVariable(ClrRaiseDebugLineUpdated));
            Debug.Assert(_python.ContainsVariable(ClrRobotPath));
           
            await Task.Factory.StartNew(() =>
            {
                _python.SetVariable(ClrTryCancelExection, () => token.ThrowIfCancellationRequested());
                _python.Engine.ExecuteFile(pythonOptions.Value.SetLineUpdatingPath, _python);

                DebugLineUpdated += (_, _) =>
                {
                    sleepHelper.Sleep(
                        _actualSpeed,
                        speedOptions.Value.SleepChunk,
                        token);
                };
                
                _python.Engine.CreateScriptSourceFromString(code, "<string>").Execute(_python);

                if (!pythonSettingsStorage.Current.UseEntryFunction)
                    return;

                var main = _python.GetVariable(pythonSettingsStorage.Current.EntryFunctionName);
                main();
                
            }, TaskCreationOptions.LongRunning);
        }
        catch (Exception e) when (e is OperationCanceledException || e.InnerException is OperationCanceledException)
        {
            logger.LogDebug("Code execution was canceled");
        }
        catch (Exception e) when (IsPythonException(e))
        {
            CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs { Text = e.Message });
            logger.LogWarning(e, "Python execution error: {ErrorType}", e.GetType().Name);
        }
        catch (Exception e)
        {
            CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs { Text = $".NET error: {e}" });
            logger.LogError(e, "Unexpected error during Python execution");
        }
    }
    

    public void Dispose()
    {
        ClearEvents();
    }

    public async ValueTask DisposeAsync()
    {
        ClearEvents();
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
        return (e.GetType().Namespace ?? string.Empty).Contains(nameof(IronPython));
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
