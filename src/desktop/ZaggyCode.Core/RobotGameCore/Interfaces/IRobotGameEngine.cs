using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Core.RobotGameCore.Interfaces;

public interface IRobotGameEngine
{
    RobotGameState State { get; }
    Task<RobotGameResult> ExecuteAsync(string code, CancellationToken cancellationToken = default, ExecutionType executionType = ExecutionType.ExecuteWithProxies);
}
