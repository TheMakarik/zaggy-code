namespace ZaggyCode.Modules.Game;

//#:NO_AI
public sealed class GameEngine : IGameEngine
{
    private Task? _ioRedirectingTask;
    private Task? _loadModulesTask;
    private Task? _enableLineUpdatingTask;
    private CancellationToken? _backgroundLoadingCancellationToken;
    
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
    public EventHandler<RobotPointUpdatedEventArgs> RobotPointUpdated { get; set; }
    public EventHandler PlayerDies { get; set; }
    public EventHandler<OverrodeGameComponentEventArgs> OverrodeGameComponent { get; set; }
    public ExecutionSpeed Speed { get; set; }
    public TextReader Input { get; set; }
    public TextWriter Output { get; set; }
    public Language Language { get; set; }
    
    public async Task RunCode(string code, CancellationToken token)
    {
        throw new NotImplementedException();
    }


    private void StartIoRedirecting()
    {
        _ioRedirectingTask = Task.Run(() =>
        {

        });
    }
}