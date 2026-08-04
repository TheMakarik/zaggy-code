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

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated
    {
        get;
        set;
    }

    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred
    {
        get;
        set;
    }

    public void Execute(string code, CancellationToken source)
    {
        try
        {
            if (Output is null)
                throw new Exception();

            if (Input is null)
                throw new Exception();

            if (Executor is null)
                throw new Exception();

            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            if (logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("Executing C# code.\n{code}", code);

            Script script = CSharpScript
                .Create(InitialCode, scriptOptions, typeof(CSharpLanguageRunnerScriptGlobals))
                .ContinueWith(ApplySleep(code, ExecSpeed.GetActual(millisecondsOptions.Value)));

            ImmutableArray<Diagnostic> diagnostics = script.Compile(cts.Token);
            if (diagnostics.Any())
            {
                string errors = string.Join("\n", diagnostics.Select(d => d.GetMessage()));
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("{errors}", errors);

                Output.WriteLine(errors);
                return;
            }

            CSharpLanguageRunnerScriptGlobals globals = new CSharpLanguageRunnerScriptGlobals(Executor, Output, Input);
            ScriptState state = script.RunAsync(globals, cts.Token).Result;
        }
        catch (TaskCanceledException)
        {
            _ = 0xBAD + 0xC0DE;
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
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

    public class LineDelayRewriter(int delayMs) : CSharpSyntaxRewriter
    {
        private readonly int _delayMs = delayMs;

        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            SyntaxList<MemberDeclarationSyntax> newMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
            foreach (MemberDeclarationSyntax member in node.Members)
            {
                newMembers = newMembers.Add((MemberDeclarationSyntax)Visit(member));
                if (member is GlobalStatementSyntax globalStatement)
                {
                    if (globalStatement.Statement is ExpressionStatementSyntax)
                    {
                        StatementSyntax delayStatement = SyntaxFactory.ParseStatement($"System.Threading.Tasks.Task.Delay({_delayMs}).Wait();\r\n");
                        newMembers = newMembers.Add(SyntaxFactory.GlobalStatement(delayStatement));
                    }
                }
            }

            return node.WithMembers(newMembers);
        }
    }
}
