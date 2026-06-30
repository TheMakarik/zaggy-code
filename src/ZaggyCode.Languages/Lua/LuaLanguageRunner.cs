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
public sealed class LuaLanguageRunner(ILogger<LuaLanguageRunner> logger, IOptions<SpeedMillisecondsOptions> options) : ILanguageRunner
{
    private readonly NLua.Lua _lua = new();
    
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    
    public void RedirectIoStreams(TextReader input, TextWriter output)
    {
        _lua["__clr_input"] = input;
        _lua["__clr_output"] = output;
        _lua.DoString(@"

io.read = function()
    return __clr_input:ReadLine()
end

io.write = function(text)
    __clr_output:Write(tostring(text))
end

print = function(text)
     __clr_output:WriteLine(tostring(text))
end

");
    }

    public void Execute(string code, ExecutionSpeed speed, IRobotMover mover)
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