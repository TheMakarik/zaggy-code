using Microsoft.Extensions.Logging;
using ShardScript.Application;
using ShardScript.Runtime;
using ShardScript.Scripting;
using ShardScript.Syntax;
using ShardScript.Syntax.Builders;
using ShardScript.Syntax.Symbols;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;
using ZaggyCode.Languages.Interfaces;

namespace ZaggyCode.Languages.ShardScript;

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
        NamespaceSymbol ns = SymbolBuilder.Namespace(context, "zaggy").Build();
        TypeSymbol robotClass = SymbolBuilder.Class(context, "Robot", ns).Build();

        SymbolBuilder.Method(context, robotClass, "Up", SymbolBuilder.Primitive(context, PrimitiveType.Void)).Public().Static().Callback((method, args, argsCount, userData, collector) =>
        {
            mover.Up();
            return IntPtr.Zero;
        });

        SymbolBuilder.Method(context, robotClass, "Down", SymbolBuilder.Primitive(context, PrimitiveType.Void)).Public().Static().Callback((method, args, argsCount, userData, collector) =>
        {
            mover.Down();
            return IntPtr.Zero;
        });

        SymbolBuilder.Method(context, robotClass, "Left", SymbolBuilder.Primitive(context, PrimitiveType.Void)).Public().Static().Callback((method, args, argsCount, userData, collector) =>
        {
            mover.Left();
            return IntPtr.Zero;
        });

        SymbolBuilder.Method(context, robotClass, "Right", SymbolBuilder.Primitive(context, PrimitiveType.Void)).Public().Static().Callback((method, args, argsCount, userData, collector) =>
        {
            mover.Right();
            return IntPtr.Zero;
        });
    }

    private static void RegisterIo(CompilationContext context, TextWriter? output, TextReader? input)
    {
        NamespaceSymbol ns = SymbolBuilder.Namespace(context, "zaggy").Build();
        TypeSymbol consoleClass = SymbolBuilder.Class(context, "Console", ns).Build();

        if (output != null)
        {
            SymbolBuilder.Method(context, consoleClass, "WriteLine", SymbolBuilder.Primitive(context, PrimitiveType.Void))
                .Public().Static().Parameter("text", SymbolBuilder.Primitive(context, PrimitiveType.String))
                .Callback((method, args, argsCount, userData, collector) =>
                {
                    string text = new ObjectInstance(args[0]).AsString();
                    output.WriteLine(text);
                    return IntPtr.Zero;
                });
        }

        if (input != null)
        {
            SymbolBuilder.Method(context, consoleClass, "ReadLineLine", SymbolBuilder.Primitive(context, PrimitiveType.String))
                .Public().Static()
                .Callback((method, args, argsCount, userData, collector) =>
                {
                    string? text = input.ReadLine();
                    if (string.IsNullOrEmpty(text))
                        return new GarbageCollector(collector).FromString("").Handle;

                    return new GarbageCollector(collector).FromString(text).Handle;
                });
        }
    }

    public void Dispose()
    {

    }
}
