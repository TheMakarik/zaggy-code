using ZaggyCode.Modules.Game.Json;

namespace ZaggyCode.Modules.Game;

public sealed class RobotGameEngine(
    IUserStorage userStorage,
    ILogger<RobotGameEngine> logger,
    IServiceScopeFactory scopeFactory,
    IRobotExecutorFactory robotExecutorFactory) : IRobotGameEngine
{
    private readonly Lock _guard = new();
    private CancellationTokenSource _engineCts = new();
    private IServiceScope? _scope;
    private ILanguageRunner _languageRunner = null!;
    private TaskCompletionSource _runnerReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Task? _registerLanguageTask;
    private TextWriter? _output;
    private TextReader? _input;

    private Task? _setSpeedTask;
    private Task? _setIoTask;
    private Task? _loadModulesTask;

    public ExecutionSpeed Speed
    {
        get;
        set
        {
            SetValue(out field, value);
            StartSetSpeedTask();
        }
    } = userStorage.Current.LastSpeed;

    public Map? CurrentMap
    {
        get;
        set
        {
            SetValue(out field, value);
            StartLoadingModulesTask();
        }
    }

    public Language Language
    {
        get;
        set
        {
            SetValue(out field, value);
            StartRegisterLanguageTask(value);
        }
    } = userStorage.Current.LastLanguage;

    public void SetIo(TextWriter output, TextReader input)
    {
        using (var @lock = _guard.EnterScope())
        {
            _output = output;
            _input = input;
        }

        StartSetIoTask();
    }

    public async Task RunCodeAsync(string code, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private void SetValue<T>(out T target, T value)
    {
        using var @lock = _guard.EnterScope();
        target = value;
    }

    private void StartSetSpeedTask()
    {
        EnsureRegisterTaskStarted();
        _setSpeedTask = RunAfterRunnerReady((runner, token) =>
            runner.SetSpeed(Speed, token), nameof(_setSpeedTask));
    }

    private void StartSetIoTask()
    {
        EnsureRegisterTaskStarted();
        _setIoTask = RunAfterRunnerReady((runner, token) =>
        {
            Debug.Assert(_input is not null && _output is not null);
            runner.RedirectIo(_input!, _output!, token);
        }, nameof(_setIoTask));
    }

    private void StartLoadingModulesTask()
    {
        EnsureRegisterTaskStarted();
        _loadModulesTask = RunAfterRunnerReady((runner, token) =>
        {
            var map = CurrentMap;
            if (map is null)
                return;

            var executor = robotExecutorFactory.GetFactory(map);
            SubscribeToRobotExecutorEvents(executor);
            runner.SetExecutor(executor, token);
        }, nameof(_loadModulesTask));
    }

    private void StartRegisterLanguageTask(Language language)
    {
        CancellationTokenSource previousToken;
        CancellationTokenSource generationToken;
        TaskCompletionSource generationReady;

        using (var @lock = _guard.EnterScope())
        {
            previousToken = _engineCts;
            generationToken = new CancellationTokenSource();
            generationReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _engineCts = generationToken;
            _runnerReady = generationReady;
            _scope?.Dispose();
            _scope = null;
        }

        previousToken.Cancel();
        previousToken.Dispose();

        using (var @lock = _guard.EnterScope())
        {
            _registerLanguageTask = LogTaskExecution(
                Task.Factory.StartNew(() => RegisterLanguage(language, generationToken.Token, generationReady), generationToken.Token),
                nameof(_registerLanguageTask));
        }
    }

    private void EnsureRegisterTaskStarted()
    {
        using var @lock = _guard.EnterScope();
        if (_registerLanguageTask is not null)
            return;

        var initialLanguage = Language;
        CancellationTokenSource generationCts = new();
        TaskCompletionSource generationReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _engineCts = generationCts;
        _runnerReady = generationReady;
        _registerLanguageTask = LogTaskExecution(
            Task.Factory.StartNew(() => RegisterLanguage(initialLanguage, generationCts.Token, generationReady), generationCts.Token),
            nameof(_registerLanguageTask));
    }

    private void RegisterLanguage(Language language, CancellationToken token, TaskCompletionSource ready)
    {
        var scope = scopeFactory.CreateScope();
        try
        {
            token.ThrowIfCancellationRequested();
            logger.LogDebug("Start searching language runner for {language}", language);
            var runner = scope.ServiceProvider.GetRequiredKeyedService<ILanguageRunner>(language);

            token.ThrowIfCancellationRequested();

            using (var @lock = _guard.EnterScope())
            {
                _scope = scope;
                _languageRunner = runner;
            }

            logger.LogDebug("Successfully create language runner instance");
            ApplyRunnerConfiguration(token);
            ready.TrySetResult();
        }
        catch (OperationCanceledException)
        {
            ready.TrySetCanceled();
            ClearPublishedScope(scope);
        }
        catch (Exception ex)
        {
            ready.TrySetException(ex);
            ClearPublishedScope(scope);
            logger.LogError(ex, "Failed to create language runner for {language}", language);
        }
    }

    private void ApplyRunnerConfiguration(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _languageRunner.SetSpeed(Speed, token);

        if (_input is not null && _output is not null)
            _languageRunner.RedirectIo(_input, _output, token);

        var map = CurrentMap;
        if (map is null) 
            return;
        
        token.ThrowIfCancellationRequested();
        var executor = robotExecutorFactory.GetFactory(map);
        SubscribeToRobotExecutorEvents(executor);
        _languageRunner.SetExecutor(executor, token);
    }

    private void ClearPublishedScope(IServiceScope scope)
    {
        using var @lock = _guard.EnterScope();
        if (ReferenceEquals(_scope, scope))
            _scope = null;

        scope.Dispose();
    }

    private Task RunAfterRunnerReady(Action<ILanguageRunner, CancellationToken> action, string taskName)
    {
        CancellationTokenSource cts;
        Task ready;

        using (var @lock = _guard.EnterScope())
        {
            cts = _engineCts;
            ready = _runnerReady.Task;
        }

        return LogTaskExecution(Task.Run(async () =>
        {
            try
            {
                await ready.WaitAsync(cts.Token);
                if (cts.IsCancellationRequested)
                    return;

                ILanguageRunner runner;
                using (var @lock = _guard.EnterScope())
                {
                    if (cts.IsCancellationRequested)
                        return;

                    runner = _languageRunner;
                }
                
                action(runner, cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }), taskName);
    }

    private void SubscribeToRobotExecutorEvents(IRobotExecutor executor)
    {
        executor.DrawPoint += (_, _) =>
        {
        };
        executor.RobotDied += (_, _) =>
        {
        };
        executor.RobotPointUpdated += (_, _) =>
        {
        };
    }

    private Task LogTaskExecution(Task task, string taskName)
    {
        return task.ContinueWith((result) =>
        {
            if (result.IsCompletedSuccessfully)
                logger.LogInformation("{name} was completed successfully", taskName);
            else if (result.IsCanceled)
                logger.LogInformation("{name} was canceled", taskName);
            else if (result.Exception is not null)
                logger.LogError(result.Exception, "{name} failed", taskName);
            else
                logger.LogCritical(
                    "Something go wrong during task execution, but nobody know that, {taskPath} JSON: {taskJson}",
                    typeof(Task).FullName,
                    JsonSerializer.Serialize(result, TaskSerializerContext.Default.Options));
        });
    }
}
