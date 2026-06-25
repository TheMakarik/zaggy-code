using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZaggyCode.Games.Events;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;
using ZaggyCode.Languages.Interfaces;
using ZaggyCode.Languages.Options;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Languages.Lua;

[ScopedService]
[LanguageExtension(".lua")]
public sealed class LuaLanguageRunner(ILogger logger, IOptions<SpeedMillisecondsOptions> speedOptions) : ILanguageRunner
{
    private NLua.Lua _lua = new NLua.Lua();
    private volatile bool _isLoadedRobot = false;
    
    public EventHandler<DebugLineUpdatedEventArgs> DebugLineUpdated { get; set; }
    public RobotEvents Execute(string code, ExecutionSpeed speed)
    {
        Debug.Assert(Thread.CurrentThread.ManagedThreadId != 1, "This function must be called from another thread");
        
        var events = new RobotEvents();
        LoadLineUpdating(speed);
        LoadRobot(events);
        _lua.DoString(code);
        throw new NotImplementedException();
    }

    private void LoadLineUpdating(ExecutionSpeed speed)
    {
        var waitMilliseconds = speedOptions.GetType().GetProperty(speed.ToString())?.GetValue(speedOptions) as int? ?? 0;
        _lua["__raise_DebugLineUpdated_ClrEvent"] = (int line) =>
        {
            DebugLineUpdated.Invoke(this, new  DebugLineUpdatedEventArgs(){LineNumber = line});
            Thread.Sleep(waitMilliseconds);
        };
        _lua.DoString(@"
debug.sethook(function(event, lineNumber) 
    __raise_DebugLineUpdated_ClrEvent(lineNumber)

end, ""l"")
");
        logger.LogDebug("Loaded line movements hook for lua script");
    }

    private void LoadRobot(RobotEvents events)
    {
        _lua.DoString(@"
package.preload['robot'] = function() 
     local robot = {}

     function robot.move_up() 

     end 

     function robot.move_down()

     end

     function robot.move_right()

     end

     function robot.move_left()

     end

     function robot.draw()

     end

    function robot.is_drew_here()

     end

     function robot.is_free_from_up()

     end

     function robot.is_free_from_down()

     end

     function robot.is_free_from_right()

     end
     function robot.is_free_from_left()

     end
    
end
");
    }

    public void RedirectInputStream(Func<string> inputDelegate)
    {
        _lua["__clr_input"] = inputDelegate;
        _lua.DoString(@"
io.read = __clr_input;
");
    }

    public void RedirectOutputStream(Action<string> outputDelegate)
    {
        _lua["__clr_env_new_line"] = Environment.NewLine;
        _lua["__clr_output"] = outputDelegate;
        _lua.DoString($@"
io.write = __clr_output;
print = function(str) 
    assert(type(str) == 'string', 'ZAGGY_CODE_ERROR: Вывод должен быть строкой');
    io.write(str .. __clr_env_new_line);
end
");
    }

    public void RedirectErrorStreamToOutputStream()
    {
        //Lua has no stderr to redirect.
    }


    public void Dispose()
    {
         _lua?.Dispose();
         logger.LogDebug("Disposed lua script");
    }
}