namespace ZaggyCode.Modules.LanguageRuntime.CSharp;

//#:NO_AI
public record CSharpLanguageRunnerScriptGlobals(
    IRobotExecutor Robot,
    TextWriter Output,
    TextReader Input
);
