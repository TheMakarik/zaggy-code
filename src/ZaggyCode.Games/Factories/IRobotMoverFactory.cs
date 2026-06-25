using ZaggyCode.Games.Events;
using ZaggyCode.Games.Models;

namespace ZaggyCode.Games.Factories;

public interface IRobotMoverFactory
{
    public RobotMover GetFactory(RobotEvents events, Game game);
}