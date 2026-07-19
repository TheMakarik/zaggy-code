/*
namespace ZaggyCode.Core.Languages.CSharp;

[LanguageExtension(".cs")]
public sealed partial class CSharpLanguageRunner(ILogger<CSharpLanguageRunner> logger) : ILanguageRunner
{
    private const string InitialCode = "Console.SetIn(Input);\r\nConsole.SetOut(Output);\r\n";

    private static readonly ScriptOptions scriptOptions = ScriptOptions.Default
        .WithImports("ZaggyCode.Languages.CSharp", "System")
        .WithReferences([typeof(object).Assembly, typeof(Console).Assembly, typeof(CSharpLanguageRunner).Assembly, typeof(IRobotExecutor).Assembly, typeof(Task).Assembly]);

    private TextWriter? Output;
    private TextReader? Input;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated
    {
        get;
        set;
    }

    public EventHandler<CodeErrorOccurredEventArgs>? CodeErrorOccurred { get; set; }

    public void RedirectIo(TextReader input, TextWriter output)
    {
        Input = input;
        Output = output;
    }

    public void Execute(string code, ExecutionSpeed speed, IRobotExecutor executor)
    {
        try
        {
            if (Output is null)
                throw new Exception();

            if (Input is null)
                throw new Exception();

            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            if (logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("Executing C# code.\n{code}", code);

            Script script = CSharpScript
                .Create(InitialCode, scriptOptions, typeof(CSharpLanguageRunnerScriptGlobals))
                .ContinueWith(ApplySleep(code, SpeedToMilliseconds(speed)));

            ImmutableArray<Diagnostic> diagnostics = script.Compile(cts.Token);
            if (diagnostics.Any())
            {
                // ичо теперь?
                // добавь хоть ProblemReporter какой нибудь, я хз

                string errors = string.Join("\n", diagnostics.Select(d => d.GetMessage()));
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError("{errors}", errors);

                Output.WriteLine(errors);
                return;
            }

            CSharpLanguageRunnerScriptGlobals globals = new CSharpLanguageRunnerScriptGlobals(executor, Output, Input);
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

    private static string ApplySleep(string code, int delayMs)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = tree.GetRoot();

        LineDelayRewriter rewriter = new LineDelayRewriter(delayMs);
        SyntaxNode modifiedRoot = rewriter.Visit(root);
        string modifiedCode = modifiedRoot.ToFullString();

        return modifiedCode;
    }

    private static int SpeedToMilliseconds(ExecutionSpeed speed) => speed switch
    {
        ExecutionSpeed.X10 => 10,
        ExecutionSpeed.X5 => 200,
        ExecutionSpeed.X2 => 500,
        ExecutionSpeed.X1 or _ => 1000,
    };

    public void Dispose()
    {

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

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}
*/
