namespace ZaggyCode.Core.Game.Interfaces;

public interface IGameEngine
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }
    public EventHandler<PlayersPointUpdatedEventArgs> UpdatePlayerPoint { get; set; }
    public EventHandler PlayerDies { get; set; }
    public EventHandler<OverrodeGameComponentEventArgs> OverrodeGameComponent { get; set; }
    public ExecutionSpeed Speed { get; set; }
    public TextReader Input { get; set; }
    public TextWriter Output { get; set; }
    public Task RunCode(string code, Language language, CancellationToken token);
}