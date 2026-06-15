using Microsoft.Extensions.Logging;
using ZaggyCode.Games.Events;
using ZaggyCode.Languages.EventArgs;
using ZaggyCode.Languages.Interfaces;

namespace ZaggyCode.Languages.Lua;

public sealed class LuaLanguageRunner(ILogger logger) : ILanguageRunner
{
    private NLua.Lua? _lua = new NLua.Lua();
    
    public EventHandler<DebugLineUpdatedEventArgs> DebugLineUpdated { get; set; }
    public RobotEvents Execute(string code)
    {
        
    }

    public void RedirectInputStream(Func<string> inputDelegate)
    {
        throw new NotImplementedException();
    }

    public void RedirectOutputStream(Action<string> outputDelegate)
    {
        throw new NotImplementedException();
    }

    public void RedirectErrorStream(Action<string> outputDelegate)
    {
        throw new NotImplementedException();
    }


    public void Dispose()
    {
        // TODO release managed resources here
    }
}