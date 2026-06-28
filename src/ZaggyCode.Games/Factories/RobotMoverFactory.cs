using Microsoft.Extensions.Logging;
using ZaggyCode.Games.Events;
using ZaggyCode.Games.Models;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Games.Factories;

public sealed class RobotMoverFactory(ILogger<RobotMover> logger) : IRobotMoverFactory
{
    public RobotMover GetFactory(RobotEvents events, Game game)
    {
        return new RobotMover(logger, game, events);
    }
}