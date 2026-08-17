using ZaggyCode.Modules.Game;

namespace ZaggyCode.Modules.Factories;

public sealed class RobotExecutorFactory : IRobotExecutorFactory
{
    public IRobotExecutor GetFactory(Map map)
    {
        return new RobotExecutor(map);
    }
}