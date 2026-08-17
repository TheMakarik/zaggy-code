namespace ZaggyCode.Core.Game.EventArgs;

public class RobotPointUpdatedEventArgs : System.EventArgs
{
    public required int NewX { get; set; }
    public required int NewY { get; set; }
}