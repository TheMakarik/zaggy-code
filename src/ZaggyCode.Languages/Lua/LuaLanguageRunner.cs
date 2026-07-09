using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZaggyCode.Games.Events;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;
using ZaggyCode.Languages.Interfaces;
using ZaggyCode.Languages.Options;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Languages.Lua;

[LanguageExtension(".lua")]
public sealed class LuaLanguageRunner(ILogger<LuaLanguageRunner> logger,
    IOptions<SpeedMillisecondsOptions> speedOptions, 
    IOptions<LuaPathsOptions> luaOptions) : ILanguageRunner
{
    private readonly NLua.Lua _lua = new();
    
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    
    public void RedirectIoStreams(TextReader input, TextWriter output)
    {
        _lua["__clr_input"] = input;
        _lua["__clr_output"] = output;
        _lua["TABLE_CONTENT_CHECKER"] = true;
    }

    public void Execute(string code, ExecutionSpeed speed, IRobotExecutor executor)
    {
        //var speedMilliseconds = (int)options.GetType().GetProperty(speed.ToString()!)!.GetValue(speed)!;
        _lua.State.Encoding = Encoding.UTF8;
        _lua.DoString(code);
    }
    
    public void Dispose()
    {
        _lua.Dispose();
    }
}