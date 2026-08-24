namespace ZaggyCode.Modules.Game;

public sealed class GameEngine(ILogger<GameEngine> logger, IServiceScopeFactory scopeFactory, IRobotExecutorFactory robotExecutorFactory) : IGameEngine
{
    private readonly Lock _lock = new();
    private IServiceScope? _dependencyInjectionScope;
    private CancellationTokenSource? _backgroundLoadingCancellationSource;
    private ILanguageRunner? _languageRunner;
    private IRobotExecutor? _robotExecutor;
    private TextWriter? _output;
    private TextReader? _input;
    private Task? _ioRedirectingTask;
    private Task? _loadModulesTask;
    private Task? _setSpeedTask;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
    public EventHandler<RobotPointUpdatedEventArgs>? RobotPointUpdated { get; set; }
    public EventHandler<RobotDeadEventArgs>? RobotDead { get; set; }
    public EventHandler<DrawPointEventArgs>? DrawPoint { get; set; }
    public EventHandler<OverrodeGameComponentEventArgs>? OverrodeGameComponent { get; set; }

    public ExecutionSpeed Speed
    {
        get;
        set
        {
            field = value;
            StartSetSpeedTask();
        }
    }

    public Map? CurrentMap
    {
        get;
        set
        {
            field = value;
            StartLoadingModules(value);
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
        _output = output;
        _input = input;

        var runner = EnsureLanguageRunner();
        var token = EnsureCancellationSource().Token;

        logger.LogInformation("Begin background IO loading");
        _ioRedirectingTask = TrackBackground(
            Task.Run(() => runner.RedirectIo(input, output, token)),
            "IO redirection");
    }

    public async Task RunCodeAsync(string code, CancellationToken token)
    {
        Task?[] backgroundTasks = [_ioRedirectingTask, _loadModulesTask, _setSpeedTask];
        Task[] pendingTasks = [.. backgroundTasks.Where(task => task is { IsCompleted: false }).Select(task => task!)];

        if (pendingTasks.Length > 0)
            await Task.WhenAll(pendingTasks);
        else
            logger.LogDebug("No background tasks to wait");

        await EnsureLanguageRunner().ExecuteAsync(code, token);
        ReloadEngine();
    }

    private async Task TrackBackground(Task task, string name)
    {
        try
        {
            await task;
            logger.LogDebug("{Name} background task completed", name);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("{Name} background task was cancelled", name);
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Name} background task failed", name);
        }
    }

    private ILanguageRunner EnsureLanguageRunner()
        => _languageRunner ?? ReloadLanguageRunner(Language);

    private CancellationTokenSource EnsureCancellationSource()
    {
        _backgroundLoadingCancellationSource ??= new CancellationTokenSource();

        return _backgroundLoadingCancellationSource;
    }

    private ILanguageRunner ReloadLanguageRunner(Language language)
    {
        using var @lock = _lock.EnterScope();

        _backgroundLoadingCancellationSource?.Cancel();
        _backgroundLoadingCancellationSource?.Dispose();
        _backgroundLoadingCancellationSource = new CancellationTokenSource();

        _dependencyInjectionScope?.Dispose();
        _dependencyInjectionScope = scopeFactory.CreateScope();
        _languageRunner = _dependencyInjectionScope
            .ServiceProvider
            .GetRequiredKeyedService<ILanguageRunner>(language.GetLanguageExtension());

        logger.LogInformation("Loaded runner for {language}", language);
        return _languageRunner;
    }

    private void ReloadEngine()
    {
        ReloadLanguageRunner(Language);
        StartSetSpeedTask();
        StartLoadingModules(CurrentMap);

        if (_output is null || _input is null)
            return;

        SetIo(_output, _input);
    }

    private void StartSetSpeedTask()
    {
        var speed = Speed;
        var runner = EnsureLanguageRunner();
        var token = EnsureCancellationSource().Token;

        _setSpeedTask = TrackBackground(
            Task.Run(() => runner.SetSpeed(speed, token)),
            "Set speed");
    }

    private void StartLoadingModules(Map? map)
    {
        if (map is null)
            return;

        var runner = EnsureLanguageRunner();
        var token = EnsureCancellationSource().Token;

        _loadModulesTask = TrackBackground(Task.Run(() =>
        {
            var executor = robotExecutorFactory.GetFactory(map);
            _robotExecutor = executor;
            runner.SetExecutor(executor, token);

            /*
             *  При вызове этих евентов, в будущем, IGameEngine будет выполнять Python скрипты для самой карты
             *  Они могут менять свойства этой карты (например менять стенки, рисовать NPC и т.д) в конечном итоге
             *  Эти скрипты будут поставляться с самим файлом с Map
             *  Они могут на какое то время останавливать игру, ибо работают в том же потоке
             */
            executor.DrawPoint += (_, args) => DrawPoint?.Invoke(this, args);
            executor.RobotDied += (_, args) => RobotDead?.Invoke(this, args);
            executor.RobotPointUpdated += (_, args) => RobotPointUpdated?.Invoke(this, args);
            logger.LogDebug("IRobotExecutor events redirected to engine events");
        }), "Loading robot and other modules");
    }
}
