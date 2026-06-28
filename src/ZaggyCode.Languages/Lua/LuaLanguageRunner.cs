using System.Diagnostics;
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

[ScopedService]
[LanguageExtension(".lua")]
public sealed class LuaLanguageRunner : ILanguageRunner
{
    private readonly ILogger _logger;
    private readonly IOptions<SpeedMillisecondsOptions> _speedOptions;
    private readonly NLua.Lua _lua = new();
    private bool _isLoadedRobot = false;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }

    public LuaLanguageRunner(ILogger logger, IOptions<SpeedMillisecondsOptions> speedOptions)
    {
        _logger = logger;
        _speedOptions = speedOptions;
    }

    public void Execute(string code, ExecutionSpeed speed, IRobotMover mover)
    {
        Debug.Assert(Thread.CurrentThread.ManagedThreadId != 1, "This function must be called from another thread");

        var events = new RobotEvents();
        LoadLineUpdating(speed);
        LoadRobot(events);
        _lua.DoString(code);
    }

    public void RedirectIoStreams(Stream input, Stream output)
    {
        var reader = new StreamReader(input);
        var writer = new StreamWriter(output) { AutoFlush = true };

        _lua["__clr_input"] = new Func<string>(() => reader.ReadLine() ?? string.Empty);
        _lua["__clr_output"] = new Action<string>(writer.Write);
        _lua["__clr_env_new_line"] = Environment.NewLine;

        _lua.DoString(@"
            io.read = __clr_input;
            io.write = __clr_output;
            print = function(str)
                assert(type(str) == 'string', 'ZAGGY_CODE_ERROR: Вывод должен быть строкой');
                io.write(str .. __clr_env_new_line);
            end
        ");
    }

    public void Dispose()
    {
        _lua?.Dispose();
        _logger.LogDebug("Disposed lua script");
    }

    private void LoadLineUpdating(ExecutionSpeed speed)
    {
        var waitMilliseconds = _speedOptions.GetType()
            .GetProperty(speed.ToString())?
            .GetValue(_speedOptions) as int? ?? 0;

        _lua["__raise_DebugLineUpdated_ClrEvent"] = (int line) =>
        {
            DebugLineUpdated?.Invoke(this, new DebugLineUpdatedEventArgs { LineNumber = line });
            Thread.Sleep(waitMilliseconds);
        };

        _lua.DoString(@"
            debug.sethook(function(event, lineNumber)
                __raise_DebugLineUpdated_ClrEvent(lineNumber)
            end, 'l')
        ");

        _logger.LogDebug("Loaded line movements hook for lua script");
    }

    private void LoadRobot(RobotEvents events)
    {
        _lua.DoString(@"
            package.preload['robot'] = function()
                local robot = {}

                function robot.move_up() end
                function robot.move_down() end
                function robot.move_right() end
                function robot.move_left() end
                function robot.draw() end
                function robot.is_drew_here() end
                function robot.is_free_from_up() end
                function robot.is_free_from_down() end
                function robot.is_free_from_right() end
                function robot.is_free_from_left() end

                return robot
            end
        ");
    }
}