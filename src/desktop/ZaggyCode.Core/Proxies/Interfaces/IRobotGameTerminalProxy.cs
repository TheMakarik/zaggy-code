namespace ZaggyCode.Core.Proxies.Interfaces;

public interface IRobotGameTerminalProxy
{
    TextWriter Output { get; }
    TextReader Input { get; }
    void WriteError(string text);
}
