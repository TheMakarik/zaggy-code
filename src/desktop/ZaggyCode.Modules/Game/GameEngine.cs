namespace ZaggyCode.Modules.Game;

//#:NO_AI
public sealed class GameEngine(ILogger<GameEngine> logger, IServiceScopeFactory scopeFactory, IRobotExecutorFactory robotExecutorFactory) : IGameEngine
{
    private Task? _ioRedirectingTask;
    private Task? _loadModulesTask;
    private Task? _setSpeedTask;
    private CancellationTokenSource? _backgroundLoadingCancellationSource;
    private IServiceScope? _dependecyInjectionScope;
    private readonly Lock _lock = new Lock();
    private ILanguageRunner? _languageRunner;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
    public EventHandler<RobotPointUpdatedEventArgs>? RobotPointUpdated { get; set; }
    public EventHandler? PlayerDies { get; set; }
    public EventHandler<OverrodeGameComponentEventArgs>? OverrodeGameComponent { get; set; }

    public ExecutionSpeed Speed
    {
        get;
        set
        {
            StartSetSpeedTask();
            field = value;
        }
    }
    
    public Language Language
    {
        get;
        set
        {
            ReloadLanguageRunner(value);
            field = value;
        }
    }

    public void SetIo(TextWriter output, TextReader input)
    {
        Debug.Assert(_backgroundLoadingCancellationSource is not null);
        _ioRedirectingTask = HandleBackgroundTaskErrors(Task.Run(() =>
        {
            logger.LogInformation("Begin background IO loading");
            Debug.Assert(_languageRunner is not null);
            Debug.Assert(_backgroundLoadingCancellationSource is not null);
            _languageRunner.RedirectIo(input, output, _backgroundLoadingCancellationSource.Token);
        }, _backgroundLoadingCancellationSource.Token), "Loading IO redirection");

    }
    
    public async Task RunCodeAsync(string code, CancellationToken token)
    {
        var taskToWait = ((IEnumerable<Task?>)[_ioRedirectingTask, _loadModulesTask, _setSpeedTask])
            .Where(task => task is not null)
            .Where(task => !task!.IsCompleted)
            .Cast<Task>()
            .ToArray()
            .AsReadOnly();

        if (taskToWait.Any())
            await Task.WhenAll(taskToWait);
        else 
            logger.LogDebug("No background task to wait");

        await _languageRunner!.ExecuteAsync(code, token);
        ReloadLanguageRunner(Language);
    }
    
    private void StartLoadingModulesBackground()
    {
        Debug.Assert(_backgroundLoadingCancellationSource is not null);
        _loadModulesTask = HandleBackgroundTaskErrors(Task.Run(() =>
        {
            logger.LogInformation("Start background loading modules");
        }), "Loading modules");
    }
    
    private Task HandleBackgroundTaskErrors(Task runTask, string taskNameForLogging)
    {
        return runTask.ContinueWith(task =>
        {
            if (task.IsCanceled)
                logger.LogDebug("{name} background was cancelled", taskNameForLogging);
            else if (task.IsCompletedSuccessfully)
                logger.LogDebug("{name} background was successfully", taskNameForLogging);
            else if (task.Exception is not null)
                logger.LogDebug("{name} background stopped with exception", taskNameForLogging);
        });
    }
    
    private void ReloadLanguageRunner(Language language)
    {
        using var @lock = _lock.EnterScope();
        _backgroundLoadingCancellationSource?.Cancel();
        _backgroundLoadingCancellationSource = new CancellationTokenSource();
        _dependecyInjectionScope?.Dispose();
        _dependecyInjectionScope = scopeFactory.CreateScope();
        _languageRunner = _dependecyInjectionScope
            .ServiceProvider
            .GetRequiredKeyedService<ILanguageRunner>(language);
        
        logger.LogInformation("Loaded runner for {language}", language);
        StartLoadingModulesBackground();
    }
    
    private void StartSetSpeedTask()
    {
        Debug.Assert(_backgroundLoadingCancellationSource is not null);
        _setSpeedTask = HandleBackgroundTaskErrors(Task.Run(() =>
        {
            _languageRunner!.SetSpeed(Speed, _backgroundLoadingCancellationSource.Token);
        }), "Set speed");
    }
    
}