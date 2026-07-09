namespace ZaggyCode.Core.Languages.Interfaces;

public interface ILanguageRunner : IDisposable
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public void RedirectIoStreams(TextReader input, TextWriter output);
    public void Execute(string code, ExecutionSpeed speed, IRobotExecutor executor);
}