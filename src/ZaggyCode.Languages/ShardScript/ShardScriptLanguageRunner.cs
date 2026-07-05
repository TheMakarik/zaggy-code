using Microsoft.Extensions.Logging;
using ShardScript.Application;
using ShardScript.Scripting;
using ShardScript.Syntax;
using ShardScript.Syntax.Builders;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;
using ZaggyCode.Languages.Interfaces;

namespace ZaggyCode.Languages.ShardScript;

[LanguageExtension(".ss")]
public class ShardScriptLanguageRunner(ILogger<ShardScriptLanguageRunner> logger) : ILanguageRunner
{
    private static readonly ShardScriptOptions _options = ShardScriptOptions.Default;

    private readonly ILogger<ShardScriptLanguageRunner> _logger = logger;

    private TextWriter? _writer = null;
    private TextReader? _reader = null;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }

    public void Execute(string code, ExecutionSpeed speed, IRobotMover mover)
    {
        using ShardScriptState state = new ShardScriptState(_options);
        RegisterIo(state.Context, _writer, _reader);
        RegisterRobot(state.Context, mover);

        state.AddSource("user_script", code);
        if (!state.TryCompile())
        {
            // ачо делать то
            return;
        }

        state.Run();
    }

    public void RedirectIoStreams(TextReader input, TextWriter output)
    {
        _writer = output;
        _reader = input;
    }

    private static void RegisterRobot(CompilationContext context, IRobotMover mover)
    {
        SymbolBuilder.CreateNamespace(context, "zaggy", ns =>
        {
            ns.WithClass("Robot", cls =>
            {
                cls.Public();

                cls.WithMethod("Up", () => mover.Up());
                cls.WithMethod("Down", () => mover.Down());
                cls.WithMethod("Left", () => mover.Left());
                cls.WithMethod("Right", () => mover.Right());
            });
        });
    }

    private static void RegisterIo(CompilationContext context, TextWriter? output, TextReader? input)
    {
        SymbolBuilder.CreateNamespace(context, "zaggy", ns =>
        {
            ns.WithClass("Console", cls =>
            {
                cls.Public();

                cls.WithMethod("WriteLine", (string text) => output?.WriteLine(text));
                cls.WithMethod("ReadLine", () => input?.ReadLine() ?? "");
            });
        });
    }

    public void Dispose()
    {

    }
}
