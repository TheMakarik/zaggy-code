namespace ZaggyCode.Core.GamesRuntime.EventArgs;

public sealed class RobotEvents
{
    public EventHandler<RobotMovedEventArgs>? RobotMoved;
    public EventHandler? RobotDead;
    public EventHandler? LevelCompleted;
}
