using ShardScript.Syntax.Builders;

namespace ZaggyCode.Core.Languages.ShardScript;

[LanguageExtension(".ss")]
public class ShardScriptLanguageRunner(ILogger<ShardScriptLanguageRunner> logger) : ILanguageRunner
{
    private static readonly ShardScriptOptions _options = ShardScriptOptions.Default;

    private readonly ILogger<ShardScriptLanguageRunner> _logger = logger;

    private TextWriter? _writer = null;
    private TextReader? _reader = null;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }

    public void Execute(string code, ExecutionSpeed speed, IRobotExecutor executor)
    {
        using ShardScriptState state = new ShardScriptState(_options);
        RegisterIo(state.Context, _writer, _reader);
        RegisterRobot(state.Context, executor);

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

    private static void RegisterRobot(CompilationContext context, IRobotExecutor executor)
    {
        SymbolBuilder.CreateNamespace(context, "Robot", ns =>
        {
            ns.WithClass("Robot", cls =>
            {
                cls.Public();
                
                cls.WithMethod("MoveUp", executor.MoveUp);
                cls.WithMethod("MoveDown", executor.MoveDown);
                cls.WithMethod("MoveLeft", executor.MoveLeft);
                cls.WithMethod("MoveRight", executor.MoveRight);
                cls.WithMethod("Draw", executor.Draw);
                cls.WithMethod("CanMoveUp", executor.CanMoveUp);
                cls.WithMethod("CanMoveDown", executor.CanMoveDown);
                cls.WithMethod("CanMoveLeft", executor.CanMoveLeft);
                cls.WithMethod("CanMoveRight", executor.CanMoveRight);
                cls.WithMethod("IsWallFromUp", executor.IsWallFromUp);
                cls.WithMethod("IsWallFromDown", executor.IsWallFromDown);
                cls.WithMethod("IsWallFromLeft", executor.IsWallFromLeft);
                cls.WithMethod("IsWallFromRight", executor.IsWallFromRight);
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
