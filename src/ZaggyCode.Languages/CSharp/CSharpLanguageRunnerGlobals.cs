using ZaggyCode.Games.Interfaces;

namespace ZaggyCode.Languages.CSharp;

public record CSharpLanguageRunnerScriptGlobals(
    IRobotExecutor Robot,
    TextWriter Output,
    TextReader Input
);