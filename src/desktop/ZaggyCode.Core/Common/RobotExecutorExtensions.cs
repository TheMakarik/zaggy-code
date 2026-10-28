using ZaggyCode.Core.GamesRuntime.Attributes;

namespace ZaggyCode.Core.Common;

public static class RobotExecutorExtensions
{
    public static ReadOnlyCollection<MethodInfo> GetMethodsToUseInRobot(this IRobotExecutor executor)
    {
        return typeof(IRobotExecutor)
            .GetMethods()
            .Where(method => method.GetCustomAttribute<AllowUseInRobotAttribute>() is not null)
            .ToArray()
            .AsReadOnly();
    }
}