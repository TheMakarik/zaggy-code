using ZaggyCode.Games.EventArgs;

namespace ZaggyCode.Games.Events;

public sealed class RobotEvents
{
   public EventHandler<RobotMovedEventArgs> RobotMoved;
   public EventHandler RobotDead;
}