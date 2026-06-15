using ZaggyCode.Games.Events;
using ZaggyCode.Languages.EventArgs;

namespace ZaggyCode.Languages.Interfaces;

public interface ILanguageRunner : IDisposable
{
    public EventHandler<DebugLineUpdatedEventArgs> DebugLineUpdated { get; set; }
    public RobotEvents Execute(string code);
    public void RedirectInputStream(Func<string> inputDelegate);
    public void RedirectOutputStream(Action<string> outputDelegate);
    public void RedirectErrorStream(Action<string> outputDelegate);
}