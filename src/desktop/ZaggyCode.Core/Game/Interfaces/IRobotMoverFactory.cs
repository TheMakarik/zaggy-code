using ZaggyCode.Core.Game.Events;

namespace ZaggyCode.Core.Game.Interfaces;

public interface IRobotMoverFactory
{
    public IRobotExecutor GetFactory(RobotEvents events, Models.Game game);
}
