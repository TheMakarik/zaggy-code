namespace ZaggyCode.Core.Game.EventArgs;

public sealed class RobotDeadEventArgs
{
    public required RobotDiesType DiesType { get; set; }
}