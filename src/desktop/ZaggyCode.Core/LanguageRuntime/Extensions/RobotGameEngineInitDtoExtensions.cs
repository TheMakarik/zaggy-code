using ZaggyCode.Core.LanguageRuntime.Dto;
using ZaggyCode.Core.RobotGameCore.Dto;

namespace ZaggyCode.Core.LanguageRuntime.Extensions;

public static class RobotGameEngineInitDtoExtensions
{
    public static LanguageRunnerDto ToLanguageRunnerDto(this RobotGameEngineInitDto dto)
        => new()
        {
            Input = dto.Input,
            Output = dto.Output,
            Speed = dto.Speed
        };
}
