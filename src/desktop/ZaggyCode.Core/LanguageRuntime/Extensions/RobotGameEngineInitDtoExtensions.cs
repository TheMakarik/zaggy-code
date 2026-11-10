using ZaggyCode.Core.LanguageRuntime.Dto;
using ZaggyCode.Core.RobotGameCore.Dto;
using ZaggyCode.Core.RobotGameCore.Interfaces;

namespace ZaggyCode.Core.LanguageRuntime.Extensions;

public static class RobotGameEngineInitDtoExtensions
{
    public static LanguageRunnerDto ToLanguageRunnerDto(this RobotGameEngineInitDto dto, IRobotController robotController)
        => new()
        {
            Input = dto.Input,
            Output = dto.Output,
            Speed = dto.Speed,
            RobotController = robotController
        };
}
