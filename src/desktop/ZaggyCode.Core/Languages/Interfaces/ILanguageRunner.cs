namespace ZaggyCode.Core.Languages.Interfaces;

//#:NO_AI
public interface ILanguageRunner : IDisposable, IAsyncDisposable
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public ILanguageRunner RedirectIo(TextReader input, TextWriter output);
    public ILanguageRunner SetSpeed(ExecutionSpeed speed);
    public ILanguageRunner SetExecutor(IRobotExecutor executor);
    public Task Execute(string code, CancellationToken token);
}
