namespace ZaggyCode.Modules.Game;

public sealed class RobotGameEngine(
    ILogger<RobotGameEngine> logger,
    IServiceScopeFactory scopeFactory,
    IRobotExecutorFactory robotExecutorFactory) : IRobotGameEngine
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
    public ExecutionSpeed Speed { get; set; }
    public Map? CurrentMap { get; set; }
    public Language Language { get; set; }
    public void SetIo(TextWriter output, TextReader input)
    {
        throw new NotImplementedException();
    }

    public async Task RunCodeAsync(string code, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}