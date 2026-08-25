namespace ZaggyCode.Core.Game.Interfaces;

public interface IRobotGameTerminalProxy
{
    public TextWriter Output { get; }
    public TextReader Input { get; }
    public void WriteError(string text);
}