namespace ZaggyCode.Core.GamesRuntime.EventArgs;

public sealed class RobotDeadEventArgs
{
    public required RobotDeadType DeadType { get; set; }
}