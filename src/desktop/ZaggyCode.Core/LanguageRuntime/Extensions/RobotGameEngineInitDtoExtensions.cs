namespace ZaggyCode.Core.LanguageRuntime.Extensions;

public static class RobotGameEngineInitDtoExtensions
{
    public static LanguageRunnerDto ToLanguageRunnerDto(this RobotGameEngineInitDto dto, IRobotController robotController)
        => new()
        {
            Language = dto.Language,
            Input = dto.Input,
            Output = dto.Output,
            Speed = dto.Speed,
            RobotController = robotController
        };
}
