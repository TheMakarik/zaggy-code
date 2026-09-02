namespace ZaggyCode.Core.Game.Interfaces;

//#:NO_AI
public interface IRobotGameEngine
{
    public ExecutionSpeed Speed { get; set; }
    public Map? CurrentMap { get; set; }
    public Language Language { get; set; }
    public void SetIo(TextWriter output, TextReader input);
    public Task RunCodeAsync(string code, CancellationToken token);
}