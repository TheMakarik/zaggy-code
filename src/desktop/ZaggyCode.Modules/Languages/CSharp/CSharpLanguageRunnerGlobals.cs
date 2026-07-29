using ZaggyCode.Core.Game.Interfaces;

namespace ZaggyCode.Modules.Languages.CSharp;

//#:NO_AI
public record CSharpLanguageRunnerScriptGlobals(
    IRobotExecutor Robot,
    TextWriter Output,
    TextReader Input
);
