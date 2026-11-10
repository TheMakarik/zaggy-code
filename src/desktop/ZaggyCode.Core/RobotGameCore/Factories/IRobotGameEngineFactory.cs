using ZaggyCode.Core.RobotGameCore.Dto;
using ZaggyCode.Core.RobotGameCore.Interfaces;

namespace ZaggyCode.Core.RobotGameCore.Factories;

public interface IRobotGameEngineFactory
{
    Task<IRobotGameEngine> CreateAsync(RobotGameEngineInitDto dto, CancellationToken cancellationToken = default);
}
