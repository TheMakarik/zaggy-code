namespace ZaggyCode.Core.Game.Interfaces;

public interface IRobotMoverFactory
{
    public IRobotExecutor GetFactory(RobotEvents events, Models.Game game);
}
