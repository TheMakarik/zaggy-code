namespace ZaggyCode.Modules.Game;

//#:NO_AI
public sealed class GameEngine : IGameEngine
{
    private Task? _ioRedirectingTask;
    private Task? _loadModulesTask;
    private Task? _enableLineUpdatingTask;
    private CancellationTokenSource? _backgroundLoadingCancellationSource;
    private IServiceScope? _languageScope;
    private Lock _lock = new Lock();
    private ILanguageRunner _languageRunner;
    private readonly ILogger<GameEngine> _logger;
    private readonly IServiceScopeFactory _factory;

    public GameEngine(ILogger<GameEngine> logger, IServiceScopeFactory factory)
    {
        _logger = logger;
        _factory = factory;

        StartLoadingModulesBackground();
    }
    
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
    public EventHandler<RobotPointUpdatedEventArgs> RobotPointUpdated { get; set; }
    public EventHandler PlayerDies { get; set; }
    public EventHandler<OverrodeGameComponentEventArgs> OverrodeGameComponent { get; set; }
    public ExecutionSpeed Speed { get; set; }

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
            _logger.LogInformation("Begin background IO loading");
            Debug.Assert(_languageRunner is not null);
            Debug.Assert(_backgroundLoadingCancellationSource is not null);
            _languageRunner.RedirectIo(input, output, _backgroundLoadingCancellationSource.Token);
        }, _backgroundLoadingCancellationSource.Token), "Loading IO redirection");

    }
    
    public async Task RunCodeAsync(string code, CancellationToken token)
    {
        var taskToWait = ((IEnumerable<Task?>)[_ioRedirectingTask, _loadModulesTask, _enableLineUpdatingTask])
            .Where(task => task is not null)
            .Where(task => !task!.IsCompleted)
            .Cast<Task>()
            .ToArray()
            .AsReadOnly();

        if (taskToWait.Any())
            await Task.WhenAll(taskToWait);
        else 
            _logger.LogDebug("No background task to wait");

        await _languageRunner.Execute(code, token);
    }
    
    private void StartLoadingModulesBackground()
    {
        Debug.Assert(_backgroundLoadingCancellationSource is not null);
        _loadModulesTask = HandleBackgroundTaskErrors(Task.Run(() =>
        {
            _logger.LogInformation("Start background");
        }), "Loading modules");
    }
    
    private Task HandleBackgroundTaskErrors(Task runTask, string taskNameForLogging)
    {
        return runTask.ContinueWith(task =>
        {
            if (task.IsCanceled)
                _logger.LogDebug("{name} background was cancelled", taskNameForLogging);
            else if (task.IsCompletedSuccessfully)
                _logger.LogDebug("{name} background was successfully", taskNameForLogging);
            else if (task.Exception is not null)
                _logger.LogDebug("{name} background stopped with exception", taskNameForLogging);
        });
    }
    
    private void ReloadLanguageRunner(Language language)
    {
        using var @lock = _lock.EnterScope();
        _backgroundLoadingCancellationSource?.Cancel();
        _backgroundLoadingCancellationSource = new CancellationTokenSource();
        _languageScope?.Dispose();
        _languageScope = _factory.CreateScope();
        _languageRunner = _languageScope.ServiceProvider.GetRequiredKeyedService<ILanguageRunner>(language);
        _logger.LogInformation("Loaded runner for {language}", language);
    }
    
}