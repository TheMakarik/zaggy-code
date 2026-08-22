namespace ZaggyCode.Core.Languages.Interfaces;

//#:NO_AI
public interface ILanguageRunner : IDisposable, IAsyncDisposable
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public void RedirectIo(TextReader input, TextWriter output);
    public void SetSpeed(ExecutionSpeed speed);
    public void SetExecutor(IRobotExecutor executor);
    public Task Execute(string code, CancellationToken token);
}
