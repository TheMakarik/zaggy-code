using ZaggyCode.Core.RobotGameCore.Factories;
using ZaggyCode.Core.RobotGameCore.Interfaces;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotExecutorFactory : IRobotExecutorFactory
{
    public IRobotExecutor Create(RobotMap map) => new RobotExecutor(map);
}
