using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;
using ZaggyCode.Languages.Interfaces;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Languages.CSharp;

public record class CSharpLanguageRunnerScriptGlobals(
    IRobotMover Robot,
    TextWriter Output,
    TextReader Input
);

[ScopedService]
[LanguageExtension(".cs")]
public sealed class CSharpLanguageRunner(ILogger<CSharpLanguageRunner> logger) : ILanguageRunner
{
    private static readonly ScriptOptions scriptOptions = ScriptOptions.Default
        .WithImports("ZaggyCode.Languages.CSharp")
        .WithReferences([typeof(object).Assembly, typeof(IRobotMover).Assembly]);

    private TextWriter? Output;
    private TextReader? Input;

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated
    {
        get;
        set;
    }

    public void RedirectIoStreams(TextReader input, TextWriter output)
    {
        Input = input;
        Output = output;
    }

    public void Execute(string code, ExecutionSpeed speed, IRobotMover mover)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("LLM tries to execute C# code : \n{code}", code);

        try
        {
            if (Output is null)
                throw new Exception();

            if (Input is null)
                throw new Exception();

            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            CSharpLanguageRunnerScriptGlobals globals = new CSharpLanguageRunnerScriptGlobals(mover, Output, Input);
            object result = CSharpScript.EvaluateAsync(code, scriptOptions, globals, typeof(CSharpLanguageRunnerScriptGlobals), cancellationToken: cts.Token).Result;
        }
        catch (CompilationErrorException compEx)
        {
            string errors = string.Join("\n", compEx.Diagnostics.Select(d => d.GetMessage()));
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("{errors}", errors);

            //return new { status = "compilation_error", error = errors };
        }
        catch (TaskCanceledException)
        {
            //return new { status = "timeout_error", error = "Code execution exceeded the 2-minutes limit." };
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "Unhandled exception during code execution.");

            //return new { status = "runtime_error", error = ex.Message };
        }
    }

    public void Dispose()
    {

    }
}
