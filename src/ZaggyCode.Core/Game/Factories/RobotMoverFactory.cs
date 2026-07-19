namespace ZaggyCode.Core.Game.Factories;

public sealed class RobotMoverFactory(ILogger<RobotExecutor> logger) : IRobotMoverFactory
{
    public RobotExecutor GetFactory(RobotEvents events, Models.Game game)
    {
        return new RobotExecutor(logger, game, events);
    }
}