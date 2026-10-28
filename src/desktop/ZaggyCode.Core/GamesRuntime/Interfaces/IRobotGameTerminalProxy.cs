namespace ZaggyCode.Core.GamesRuntime.Interfaces;

public interface IRobotGameTerminalProxy
{
    public TextWriter Output { get; }
    public TextReader Input { get; }
    public void WriteError(string text);
}