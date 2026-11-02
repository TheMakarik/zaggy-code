using ZaggyCode.Core.LanguageRuntime.Model;
using ZaggyCode.Core.RobotGameCore.Dto;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Core.RobotGameCore.Interfaces;

public interface IRobotGameEngine
{
    public RobotGameState State { get; }
    public void Init(RobotGameEngineInitDto dto);
    
    public Task<RobotGameResult> ExecuteAsync(string code,  CancellationToken cancellationToken = default, ExecutionType executionType = ExecutionType.ExecuteWithProxies);
}