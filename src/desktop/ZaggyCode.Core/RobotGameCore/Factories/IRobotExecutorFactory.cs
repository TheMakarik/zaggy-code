using ZaggyCode.Core.RobotGameCore.Interfaces;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Core.RobotGameCore.Factories;

public interface IRobotExecutorFactory
{
    IRobotExecutor Create(RobotMap map);
}
