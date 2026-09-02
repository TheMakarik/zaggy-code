using ZaggyCode.Modules.Game.Json;

namespace ZaggyCode.Modules.Game;

public sealed class RobotGameEngine(
    IUserStorage userStorage,
    ILogger<RobotGameEngine> logger,
    IServiceScopeFactory scopeFactory,
    IRobotExecutorFactory robotExecutorFactory) : IRobotGameEngine
{
    private Task? _registerLanguageTask;
    private Task? _setIoTask;
    private Task? _setSpeedTask;
    private Task? _loadModulesTask;
    private CancellationTokenSource _restartEngineCancellationTokenSource = new CancellationTokenSource();
    private IServiceScope? _scope;
    private ILanguageRunner _langugeRunner;

    private Lock _setPropertyLock = new();
   


    public ExecutionSpeed Speed
    {
        get;
        set
        {
            SetPropertyLocked(out field, value);
            StartSetSpeedTask();
        }
    } = userStorage.Current.LastSpeed;

    public Map? CurrentMap
    {
        get;
        set
        {
            SetPropertyLocked(out field, value);
            StartLoadingModulesTask();
        }
    
    }


    public Language Language
    {
        get;
        set
        {
            SetPropertyLocked(out field, value);
            StartRegisterLanguageTask(value);
        }
     
    } = userStorage.Current.LastLanguage;
    
    public void SetIo(TextWriter output, TextReader input)
    {
        StartSetIoTask(output, input);
    }
    

    public async Task RunCodeAsync(string code, CancellationToken token)
    {
        throw new NotImplementedException();
    }
    
    private void SetPropertyLocked<T>(out T languageField, T value)
    {
        using var @lock = _setPropertyLock.EnterScope();
        languageField = value;
    }
    
    private void StartLoadingModulesTask()
    {
        _loadModulesTask = LogTaskExecution(Task.Factory.StartNew(async void () =>
        {
            Debug.Assert(CurrentMap is not null);
            
            await WaitLanguageLoadingAsync();
            var executor = robotExecutorFactory.GetFactory(CurrentMap);
            logger.LogDebug("Loading robot executor...");

            SubscribeToRobotExecutorEvents(executor);
            _langugeRunner.SetExecutor(executor, _restartEngineCancellationTokenSource.Token);
        }), nameof(_loadModulesTask));
    }

    private void SubscribeToRobotExecutorEvents(IRobotExecutor executor)
    {
        executor.DrawPoint += (_, args) =>
        {

        };
        executor.RobotDied += (_, args) =>
        {

        };
        executor.RobotPointUpdated += (_, args) =>
        {

        };
    }


    private void StartSetSpeedTask()
    {
        _setSpeedTask = LogTaskExecution(Task.Factory.StartNew(async void() =>
        {
            await WaitLanguageLoadingAsync();
            _langugeRunner.SetSpeed(Speed, _restartEngineCancellationTokenSource.Token);
        }), nameof(_setSpeedTask));
        
    }

    
    private void StartSetIoTask(TextWriter output, TextReader input)
    {
        _setIoTask = LogTaskExecution(Task.Factory.StartNew(() =>
        {
            Debug.Assert(_langugeRunner is not null);
            _langugeRunner.RedirectIo(input, output, _restartEngineCancellationTokenSource.Token);
            logger.LogDebug("Successfully redirected IO");
        }), nameof(_setIoTask));
    }
    
    private void StartRegisterLanguageTask(Language language)
    {
        _restartEngineCancellationTokenSource.Cancel();
        _restartEngineCancellationTokenSource = new();
        _scope?.Dispose();
        _registerLanguageTask = LogTaskExecution(Task.Factory.StartNew(() =>
        {
            logger.LogDebug("Start searching language runner for {language}", language);
            _scope = scopeFactory.CreateScope();
            _langugeRunner = _scope.ServiceProvider.GetRequiredKeyedService<ILanguageRunner>(language.GetLanguageExtension());
            logger.LogDebug("Successfully create language runner instance");

        }), nameof(_registerLanguageTask));
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
    
    private async ValueTask WaitLanguageLoadingAsync()
    {
        while (_registerLanguageTask is null) { } //ждемс
        if (_registerLanguageTask is not { IsCompleted: true })
            await _registerLanguageTask;
    }

}