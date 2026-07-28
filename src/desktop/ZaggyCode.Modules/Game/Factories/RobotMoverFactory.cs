using ZaggyCode.Core.Game.Events;
using ZaggyCode.Core.Game.Interfaces;

namespace ZaggyCode.Modules.Game.Factories;

public sealed class RobotMoverFactory(ILogger<RobotExecutor> logger) : IRobotMoverFactory
{
    public IRobotExecutor GetFactory(RobotEvents events, Core.Game.Models.Game game)
    {
        return new RobotExecutor(logger, game, events);
    }
}
