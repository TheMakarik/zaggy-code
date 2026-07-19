namespace ZaggyCode.Core.Game.Events;

public sealed class RobotEvents
{
   public EventHandler<RobotMovedEventArgs> RobotMoved;
   public EventHandler RobotDead;
}