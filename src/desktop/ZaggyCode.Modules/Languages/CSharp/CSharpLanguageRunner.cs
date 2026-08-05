namespace ZaggyCode.Modules.Languages.CSharp;

//#:NO_AI
[LanguageExtension(".cs"), LanguagePrettyName("C#")]
public sealed partial class CSharpLanguageRunner(ILogger<CSharpLanguageRunner> logger, IOptions<SpeedMillisecondsOptions> millisecondsOptions) : ILanguageRunner
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

    public async Task Execute(string code, CancellationToken source)
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

                await Output.WriteLineAsync(errors);
                return;
            }

            CSharpLanguageRunnerScriptGlobals globals = new CSharpLanguageRunnerScriptGlobals(Executor, Output, Input);
            ScriptState state = await script.RunAsync(globals, cts.Token);
        }
        catch (TaskCanceledException)
        {
            _ = 0xBAD + 0xC0DE;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during code execution.");
        }
    }

    public ILanguageRunner RedirectIo(TextReader input, TextWriter output)
    {
        Input = input;
        Output = output;
        return this;
    }

    public ILanguageRunner SetSpeed(ExecutionSpeed speed)
    {
        ExecSpeed = speed;
        return this;
    }

    public ILanguageRunner SetExecutor(IRobotExecutor executor)
    {
        Executor = executor;
        return this;
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }

    public void Dispose()
    {
        // TODO release managed resources here
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

    
}
