namespace ZaggyCode.Core.LanguageRuntime.Dto;

public readonly record struct LanguageRunnerDto
{
    public required Language Language { get; init; }
    public required TextReader Input { get; init; }
    public required TextWriter Output { get; init; }
    public required ExecutionSpeed Speed { get; init; }
    public required IRobotController RobotController { get; init; }
}
