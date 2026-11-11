namespace ZaggyCode.Core.RobotGameCore.Factories;

public interface IRobotGameEngineFactory
{
    Task<IRobotGameEngine> CreateAsync(RobotGameEngineInitDto dto, CancellationToken cancellationToken = default);
}
