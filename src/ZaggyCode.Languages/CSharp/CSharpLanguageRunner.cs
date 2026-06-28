using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
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

[LanguageExtension(".cs")]
public sealed partial class CSharpLanguageRunner(ILogger<CSharpLanguageRunner> logger) : ILanguageRunner
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

            code = ThreadSleepKostylAwesomeGeneratedRegex().Replace(code, "; Thread.Sleep(" + SpeedToMilliseconds(speed) + ")");
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

    [GeneratedRegex(";")]
    private static partial Regex ThreadSleepKostylAwesomeGeneratedRegex();
}
