namespace ZaggyCode.Modules.GamesRuntime.CSharp;

//#:NO_AI
public record CSharpLanguageRunnerScriptGlobals(
    IRobotExecutor Robot,
    TextWriter Output,
    TextReader Input
);
