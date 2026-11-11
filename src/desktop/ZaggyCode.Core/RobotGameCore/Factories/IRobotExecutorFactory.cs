namespace ZaggyCode.Core.RobotGameCore.Factories;

public interface IRobotExecutorFactory
{
    IRobotExecutor Create(RobotMap map);
}
