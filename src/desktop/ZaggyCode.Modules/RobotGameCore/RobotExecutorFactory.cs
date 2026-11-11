namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotExecutorFactory : IRobotExecutorFactory
{
    public IRobotExecutor Create(RobotMap map) => new RobotExecutor(map);
}
