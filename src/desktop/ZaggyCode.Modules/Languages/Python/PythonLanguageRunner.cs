using ZaggyCode.Core.Game.Interfaces;
using ZaggyCode.Core.Languages.Enums;
using ZaggyCode.Core.Languages.EventArgs;
using ZaggyCode.Core.Languages.Interfaces;

namespace ZaggyCode.Modules.Languages.Python;

//#:NO_AI
public sealed class PythonLanguageRunner : ILanguageRunner
{
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public ILanguageRunner RedirectIo(TextReader input, TextWriter output)
    {
        throw new NotImplementedException();
    }

    public ILanguageRunner SetSpeed(ExecutionSpeed speed)
    {
        throw new NotImplementedException();
    }

    public ILanguageRunner SetExecutor(IRobotExecutor executor)
    {
        throw new NotImplementedException();
    }

    public void Execute(string code, CancellationToken source)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

}
