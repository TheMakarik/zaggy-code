using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;

namespace ZaggyCode.Languages.Interfaces;

public interface ILanguageRunner : IDisposable
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public void RedirectIoStreams(TextReader input, TextWriter output);
    public void Execute(string code, ExecutionSpeed speed, IRobotMover mover);
}