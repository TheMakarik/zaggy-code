namespace ZaggyCode.Core.Game.Factories;

public interface IRobotMoverFactory
{
    public RobotExecutor GetFactory(RobotEvents events, Models.Game game);
}