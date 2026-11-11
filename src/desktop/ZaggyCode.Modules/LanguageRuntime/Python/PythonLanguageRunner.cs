namespace ZaggyCode.Modules.LanguageRuntime.Python;

[Language(Language.Python)]
public sealed class PythonLanguageRunner : LanguageRunnerBase
{
    private const string NotSupportedText = "is not supported due to your application settings";

    // Источник пользовательского кода задаётся в Execute через CreateScriptSourceFromString(code, UserCodeFileName).
    // Фреймы importlib тоже могут иметь co_filename "<string>", поэтому пользовательские фреймы
    // дополнительно помечаются переменной в globals области выполнения.
    private const string UserCodeFileName = "<string>";
    private const string LineUpdateTraceEventName = "line";
    private const string UserCodeMarkerVariable = "__zaggy_user_code__";

    private const string ClrOutput = "clr_output";
    private const string ClrInput = "clr_input";
    private const string ClrRobotPath = "robot_path";
    private const string ClrRobotControllerPrefix = "clr_RobotController_";

    private readonly ILogger<PythonLanguageRunner> _logger;
    private readonly IPythonSettingsStorage _pythonSettingsStorage;
    private readonly ScriptEngine _engine;
    private readonly ScriptScope _scope;
    private readonly Lock _actionsLock = new();
    private readonly List<RobotAction> _pendingActions = [];
    private ChannelWriter<ComputedLineOfCode>? _writer;
    private int _previousLineNumber;

    public PythonLanguageRunner(
        LanguageRunnerDto dto,
        ILogger<PythonLanguageRunner> logger,
        IOptions<PythonScriptsOptions> pythonOptions,
        IPythonSettingsStorage pythonSettingsStorage)
        : base(dto)
    {
        _logger = logger;
        _pythonSettingsStorage = pythonSettingsStorage;

        _engine = IronPython.Hosting.Python.CreateEngine();
        _scope = _engine.CreateScope();
        _scope.SetVariable(UserCodeMarkerVariable, true);

        var options = pythonOptions.Value;
        Debug.Assert(Directory.Exists(options.StandardLibraryPath));
        _engine.SetSearchPaths([options.StandardLibraryPath]);

        RegisterRobotMethods();
        _scope.SetVariable(ClrOutput, CreateOutputHandler());
        _scope.SetVariable(ClrInput, CreateInputHandler());
        _engine.ExecuteFile(options.RedirectIoPath, _scope);
        _scope.SetVariable(ClrRobotPath, options.RobotPath);
        _engine.ExecuteFile(options.PrepareModules, _scope);
    }

    public override async IAsyncEnumerable<ComputedLineOfCode> ExecuteAsync(
        string code,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<ComputedLineOfCode>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });

        var execution = Task.Factory.StartNew(
            () => ExecuteCore(code, channel.Writer, cancellationToken),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        // Отмену обрабатывает производитель: он завершает writer, а читатель
        // просто дочитывает канал. Токен здесь прервал бы enumeration исключением.
        await foreach (var line in channel.Reader.ReadAllAsync())
            yield return line;

        await execution;
    }

    private void ExecuteCore(string code, ChannelWriter<ComputedLineOfCode> writer, CancellationToken cancellationToken)
    {
        _writer = writer;
        _previousLineNumber = 0;
        ClearPendingActions();

        var executor = RobotController.Executor;
        if (executor is not null)
            executor.StateUpdated += OnExecutorStateUpdated;

        try
        {
            ValidatePythonEnvironment();
            _engine.SetTrace(CreateUserCodeTrace(writer, cancellationToken));

            _engine.CreateScriptSourceFromString(code, UserCodeFileName).Execute(_scope);

            if (_pythonSettingsStorage.Current.UseEntryFunction)
            {
                var entryPoint = _scope.GetVariable(_pythonSettingsStorage.Current.EntryFunctionName);
                entryPoint();
            }
        }
        catch (Exception exception) when (exception is OperationCanceledException || exception.InnerException is OperationCanceledException)
        {
            _logger.LogDebug("Выполнение кода отменено");
        }
        catch (Exception exception) when (IsPythonException(exception))
        {
            WriteError(writer, CodeErrorType.LanguageError, $"{exception.GetType().Name} {exception.Message}".Trim());
            _logger.LogWarning(exception, "Ошибка выполнения Python: {ErrorType}", exception.GetType().Name);
        }
        catch (Exception exception)
        {
            WriteError(writer, CodeErrorType.LanguageError, $".NET error: {exception}");
            _logger.LogError(exception, "Непредвиденная ошибка при выполнении Python");
        }
        finally
        {
            _engine.SetTrace(null);

            if (executor is not null)
                executor.StateUpdated -= OnExecutorStateUpdated;

            FlushPendingLine(writer);
            _writer = null;
            writer.TryComplete();
        }
    }

    public override ValueTask DisposeAsync()
    {
        _engine.SetTrace(null);
        _engine.Runtime.Shutdown();
        _logger.LogInformation("Исполнитель скриптов освобождён");
        return ValueTask.CompletedTask;
    }

    private void RegisterRobotMethods()
    {
        var methods = typeof(IRobotController)
            .GetMethods()
            .Where(method => method.DeclaringType == typeof(IRobotController) && !method.IsSpecialName);

        foreach (var method in methods)
        {
            var methodInfo = method;
            _scope.SetVariable(
                ClrRobotControllerPrefix + methodInfo.Name,
                () => methodInfo.Invoke(RobotController, null));
        }
    }

    private TracebackDelegate CreateUserCodeTrace(ChannelWriter<ComputedLineOfCode> writer, CancellationToken cancellationToken)
    {
        TracebackDelegate OnUserCodeLine(TraceBackFrame frame, string traceEvent, object payload)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (traceEvent == LineUpdateTraceEventName && IsUserCodeFrame(frame))
            {
                var lineNumber = Convert.ToInt32(frame.f_lineno);
                var actions = TakePendingActions();

                // Трассировка "line" срабатывает перед выполнением строки, поэтому действия
                // робота уже относятся к предыдущей строке — отдаём её потребителю вместе с ними.
                if (_previousLineNumber > 0)
                    writer.TryWrite(new ComputedLineOfCode(_previousLineNumber, new RobotStateUpdate(actions)));

                _previousLineNumber = lineNumber;
            }

            return OnUserCodeLine;
        }

        return OnUserCodeLine;
    }

    private static bool IsUserCodeFrame(TraceBackFrame frame)
        => frame.f_code.co_filename == UserCodeFileName
            && frame.f_globals.ContainsKey(UserCodeMarkerVariable);

    private void OnExecutorStateUpdated(object? sender, RobotStateUpdate update)
    {
        if (update.Actions is null)
            return;

        using var scope = _actionsLock.EnterScope();
        _pendingActions.AddRange(update.Actions);
    }

    private List<RobotAction> TakePendingActions()
    {
        using var scope = _actionsLock.EnterScope();

        if (_pendingActions.Count == 0)
            return [];

        List<RobotAction> actions = [.. _pendingActions];
        _pendingActions.Clear();
        return actions;
    }

    private void ClearPendingActions()
    {
        using var scope = _actionsLock.EnterScope();
        _pendingActions.Clear();
    }

    private void FlushPendingLine(ChannelWriter<ComputedLineOfCode> writer)
    {
        var actions = TakePendingActions();

        if (_previousLineNumber <= 0 && actions.Count == 0)
            return;

        writer.TryWrite(new ComputedLineOfCode(_previousLineNumber, new RobotStateUpdate(actions)));
        _previousLineNumber = 0;
    }

    private Action<string> CreateOutputHandler()
    {
        return text =>
        {
            if (_pythonSettingsStorage.Current.SupressIo)
            {
                WriteError(CodeErrorType.IoBlocked, $"print() {NotSupportedText}");
                return;
            }

            Output.WriteLine(text);
        };
    }

    private Func<string> CreateInputHandler()
    {
        return () =>
        {
            if (_pythonSettingsStorage.Current.SupressIo)
            {
                WriteError(CodeErrorType.IoBlocked, $"input() {NotSupportedText}");
                return string.Empty;
            }

            return Input.ReadLine() ?? string.Empty;
        };
    }

    private static void WriteError(ChannelWriter<ComputedLineOfCode> writer, CodeErrorType codeErrorType, string message)
        => writer.TryWrite(new ComputedLineOfCode(0, default, new CodeError(codeErrorType, message)));

    private void WriteError(CodeErrorType codeErrorType, string message)
    {
        var writer = _writer;
        if (writer is not null)
            WriteError(writer, codeErrorType, message);
    }

    private static bool IsPythonException(Exception exception)
    {
        var @namespace = exception.GetType().Namespace ?? string.Empty;
        return @namespace.Contains(nameof(IronPython)) || @namespace.Contains(nameof(Microsoft.Scripting));
    }

    private void ValidatePythonEnvironment()
    {
        Debug.Assert(Directory.Exists(_scope.Engine.GetSearchPaths().First()));
        Debug.Assert(_scope.ContainsVariable(ClrInput));
        Debug.Assert(_scope.ContainsVariable(ClrOutput));
        Debug.Assert(_scope.ContainsVariable(ClrRobotPath));
    }
}
