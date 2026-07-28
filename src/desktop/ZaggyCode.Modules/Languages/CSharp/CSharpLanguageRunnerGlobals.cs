using ZaggyCode.Core.Game.Interfaces;

namespace ZaggyCode.Modules.Languages.CSharp;

public record CSharpLanguageRunnerScriptGlobals(
    IRobotExecutor Robot,
    TextWriter Output,
    TextReader Input
);
