namespace ZaggyCode.Modules.Factories;

//#:NO_AI
public sealed class RobotExecutorFactory : IRobotExecutorFactory
{
    public IRobotExecutor GetFactory(Map map)
    {
        return new RobotExecutor(map);
    }
}