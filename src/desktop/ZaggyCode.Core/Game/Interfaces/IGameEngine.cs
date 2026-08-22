namespace ZaggyCode.Core.Game.Interfaces;

//#:NO_AI
public interface IGameEngine
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
    public EventHandler<RobotPointUpdatedEventArgs> RobotPointUpdated { get; set; }
    public EventHandler PlayerDies { get; set; }
    public EventHandler<OverrodeGameComponentEventArgs> OverrodeGameComponent { get; set; }
    public ExecutionSpeed Speed { get; set; }
    public Language Language { get; set; }
    public void SetIo(TextWriter output, TextReader input);
    public Task RunCodeAsync(string code, CancellationToken token);
}