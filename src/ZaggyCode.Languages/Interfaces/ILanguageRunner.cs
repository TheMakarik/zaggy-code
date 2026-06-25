using ZaggyCode.Games.Events;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;

namespace ZaggyCode.Languages.Interfaces;

public interface ILanguageRunner : IDisposable
{
    public EventHandler<DebugLineUpdatedEventArgs> DebugLineUpdated { get; set; }
    public RobotEvents Execute(string code, ExecutionSpeed speed);
    public void RedirectInputStream(Func<string> inputDelegate);
    public void RedirectOutputStream(Action<string> outputDelegate);
    public void RedirectErrorStreamToOutputStream();
}