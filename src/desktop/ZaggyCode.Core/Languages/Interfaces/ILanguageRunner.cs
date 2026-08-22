namespace ZaggyCode.Core.Languages.Interfaces;

//#:NO_AI
public interface ILanguageRunner : IDisposable, IAsyncDisposable
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public void RedirectIo(TextReader input, TextWriter output, CancellationToken token);
    public void SetSpeed(ExecutionSpeed speed, CancellationToken token);
    public void SetExecutor(IRobotExecutor executor, CancellationToken token);
    public Task Execute(string code, CancellationToken token);
}
