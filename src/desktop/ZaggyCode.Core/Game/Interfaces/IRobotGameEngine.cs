namespace ZaggyCode.Core.Game.Interfaces;

//#:NO_AI
public interface IRobotGameEngine
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
  
    public ExecutionSpeed Speed { get; set; }
    public Map? CurrentMap { get; set; }
    public Language Language { get; set; }
    public void SetIo(TextWriter output, TextReader input);
    public Task RunCodeAsync(string code, CancellationToken token);
}