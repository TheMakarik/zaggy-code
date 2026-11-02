using ZaggyCode.Core.LanguageRuntime.Enums;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Core.RobotGameCore.Dto;

public ref struct RobotGameEngineInitDto
{
    public required TextReader Input { get; init; }
    public required TextWriter Output { get; init; }
    public required Language Language { get; init; }
    public required ExecutionSpeed Speed { get; init; } 
    public required RobotGame Game { get; init; }
}