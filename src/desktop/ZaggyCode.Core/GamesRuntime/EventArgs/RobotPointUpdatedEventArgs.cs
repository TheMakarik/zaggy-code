namespace ZaggyCode.Core.GamesRuntime.EventArgs;

public class RobotPointUpdatedEventArgs : System.EventArgs
{
    public required int NewX { get; set; }
    public required int NewY { get; set; }
}