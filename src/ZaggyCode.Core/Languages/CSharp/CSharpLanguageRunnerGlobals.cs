namespace ZaggyCode.Core.Languages.CSharp;

public record CSharpLanguageRunnerScriptGlobals(
    IRobotExecutor Robot,
    TextWriter Output,
    TextReader Input
);