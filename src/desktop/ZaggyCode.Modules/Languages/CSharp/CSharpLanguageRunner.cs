using System.CodeDom.Compiler;
using System.Reflection.PortableExecutable;

namespace ZaggyCode.Modules.Languages.CSharp;

//#:NO_AI
[LanguageExtension(".cs"), LanguagePrettyName("C#")]
public sealed partial class CSharpLanguageRunner(
    ILogger<CSharpLanguageRunner> logger,
    IOptions<SpeedMillisecondsOptions> millisecondsOptions) : ILanguageRunner
{
    private const string InitialCode = "Console.SetIn(Input);\r\nConsole.SetOut(Output);\r\n";

    private static readonly ScriptOptions scriptOptions = ScriptOptions.Default
        .WithImports("ZaggyCode.Modules.Languages.CSharp", "System")
        .WithReferences([typeof(object).Assembly, typeof(Console).Assembly, typeof(CSharpLanguageRunner).Assembly, typeof(IRobotExecutor).Assembly, typeof(Task).Assembly]);

    private TextWriter? Output;
    private TextReader? Input;
    private ExecutionSpeed ExecSpeed;
    private IRobotExecutor? Executor;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }

    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public async Task ExecuteAsync(string code, CancellationToken token)
    {
        try
        {
            Debug.Assert(Output is not null);
            Debug.Assert(Input is not null);
            Debug.Assert(Executor is not null);
            
            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            Script script = CSharpScript
                .Create(InitialCode, scriptOptions, typeof(CSharpLanguageRunnerScriptGlobals))
                .ContinueWith(ApplySleep(code, ExecSpeed.GetActual(millisecondsOptions.Value)));

            ImmutableArray<Diagnostic> diagnostics = script.Compile(cts.Token);
            if (diagnostics.Any())
            {
                string errors = string.Join(Environment.NewLine, diagnostics.Select(d => d.GetMessage()));
                logger.LogError("C# Runner errors: {errors}", errors);

                CodeErrorOccurred?.Invoke(this, new CodeErrorOccurredEventArgs { Text = errors });
                await Output.WriteLineAsync(errors);
                return;
            }

            CSharpLanguageRunnerScriptGlobals globals = new CSharpLanguageRunnerScriptGlobals(Executor, Output, Input);
            ScriptState state = await script.RunAsync(globals, cts.Token);
        }
        catch (TaskCanceledException)
        {
            _ = 0xDEAD + 0xBEEF;
        }
        catch (Exception ex)
        {
            _ = 0xBAD + 0xC0DE;
            logger.LogError(ex, "Unhandled exception during code execution.");
        }
    }

    private static string ApplySleep(string code, int delayMs)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = tree.GetRoot();

        LineDelayRewriter rewriter = new LineDelayRewriter(delayMs);
        SyntaxNode modifiedRoot = rewriter.Visit(root);
        string modifiedCode = modifiedRoot.ToFullString();

        return modifiedCode;
    }

    public void RedirectIo(TextReader input, TextWriter output, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        Input = input;
        Output = output;
    }

    public void SetSpeed(ExecutionSpeed speed, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        ExecSpeed = speed;
    }

    public void SetExecutor(IRobotExecutor executor, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        Executor = executor;
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
