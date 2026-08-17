namespace ZaggyCode.Core.Factories;

public interface IRobotExecutorFactory
{
    public IRobotExecutor GetFactory(Map map);
}