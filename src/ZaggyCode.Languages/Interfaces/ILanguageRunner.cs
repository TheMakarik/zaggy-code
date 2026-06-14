using ZaggyCode.Languages.EventArgs;

namespace ZaggyCode.Languages.Interfaces;

public interface ILanguageRunner
{
    public EventHandler<DebugLineUpdatedEventArgs> DebugLineUpdated { get; set; }
    public void ExecuteAsync(string code);
    public void RedirectInputStream(Func<string> inputDelegate);
    public void RedirectOutputStream(Action<string> outputDelegate);
}