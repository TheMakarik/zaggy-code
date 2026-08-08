namespace ZaggyCode.Modules.Languages.Python;

[LanguageExtension(".py")]
public sealed class PythonLanguageRunner(
    ILogger<PythonLanguageRunner> logger,
    IOptions<PythonScriptsOptions> pythonOptions,
    IPythonSettingsStorage pythonSettingsStorage,
    IOptions<SpeedMillisecondsOptions> speedOptions,
    IPythonScopeFactory pythonScopeFactory,
    IUserStorage userStorage)
    : ILanguageRunner
{
    private const string NotSupportedText = "is not supported due to your application settings";
  
    private ScriptScope _python = pythonScopeFactory.GetFactory();

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public ILanguageRunner RedirectIo(TextReader input, TextWriter output)
    {
       _python.SetVariable("clr_output", (string text) =>
       {
           if (pythonSettingsStorage.Current.SupressIo)
           {
               CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs(){Text = $"print() {NotSupportedText}"});
               return;
           }
           
           output.WriteLine(text);
           
       });
       _python.SetVariable("clr_input", (string text) =>
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
        var actualSpeed = userStorage.Current.LastSpeed.GetActual(speedOptions.Value);
        _python.SetVariable("clr_wait_to_new_line_ms", actualSpeed);
        _python.SetVariable("clr_raise_debug_line_updated", (int line) =>
        {
            DebugLineUpdated?.Invoke(this, new DebugLineUpdatedEventArgs(){LineNumber = line});
        });
        _python.Engine.ExecuteFile(pythonOptions.Value.SetLineUpdatingPath, _python);
        logger.LogInformation("Set line updating for python with delay {ms}ms", actualSpeed);
        
        return this;
    }

    public ILanguageRunner SetExecutor(IRobotExecutor executor)
    {
        var methods = typeof(IRobotExecutor).GetMethods(BindingFlags.Public);
        logger.LogDebug("Methods for executor: [{methods}]", string.Join(",", methods));
        foreach (var methodInfo in methods)
            _python.SetVariable(FormatVariable(methodInfo.Name), methodInfo.Invoke(executor, null));
        
        _python.SetVariable("robot_path", pythonOptions.Value.RobotPath);
        _python.Engine.ExecuteFile(pythonOptions.Value.EnableRobotPath, _python);
        logger.LogInformation("Set executor for python");
        
        return this;
    }

    private string FormatVariable(string methodInfoName)
    {
        return "clr_RobotExecutor_" + methodInfoName;
    }

    public async Task Execute(string code, CancellationToken source)
    {
        try
        {
            await Task.Factory.StartNew(() =>
            {
                DebugLineUpdated += (_, _) => { source.ThrowIfCancellationRequested(); };
                _python.Engine.Execute(code, _python);

                if (!pythonSettingsStorage.Current.UseEntryFunction) 
                    return;
        
                var main = _python.GetVariable(pythonSettingsStorage.Current.EntryFunctionName);
                main();
            }, TaskCreationOptions.LongRunning);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Code execution was canceled");
        }
        catch (Exception e) when ((e.GetType().Namespace ?? string.Empty).Contains(nameof(IronPython)))
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

    private void ClearEvents()
    {
        DebugLineUpdated = null;
        CodeErrorOccurred = null;
    }

}
