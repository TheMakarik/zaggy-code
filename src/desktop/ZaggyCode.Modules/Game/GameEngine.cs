namespace ZaggyCode.Modules.Game;

//#:NO_AI
public sealed class GameEngine(ILogger<GameEngine> logger, IServiceScopeFactory factory) : IGameEngine
{
    private Task? _ioRedirectingTask;
    private Task? _loadModulesTask;
    private Task? _enableLineUpdatingTask;
    private CancellationTokenSource? _backgroundLoadingCancellationSource;
    private IServiceScope? _languageScope;
    private Lock _lock = new Lock();
    private ILanguageRunner _languageRunner;

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
            using var @lock = _lock.EnterScope();
            _backgroundLoadingCancellationSource?.Cancel();
            _backgroundLoadingCancellationSource = new CancellationTokenSource();
            _languageScope?.Dispose();
            _languageScope = factory.CreateScope();
            _languageRunner = _languageScope.ServiceProvider.GetRequiredKeyedService<ILanguageRunner>(value);
            
            field = value;
        }
    }

    public void SetIo(TextWriter output, TextReader input)
    {
        _ioRedirectingTask = Task.Run(() =>
        {       
            Debug.Assert(_languageRunner is not null);
            _languageRunner.RedirectIo(input, output);
        });
    }

    public async Task RunCodeAsync(string code, CancellationToken token)
    {
        var notNullTasks = ((IEnumerable<Task?>)[_ioRedirectingTask, _loadModulesTask, _enableLineUpdatingTask])
            .Where(task => task is not null)
            .Cast<Task>();
        await Task.WhenAll(notNullTasks);
    }


    private void StartIoRedirecting()
    {
        _ioRedirectingTask = Task.Run(() =>
        {

        });
    }
}