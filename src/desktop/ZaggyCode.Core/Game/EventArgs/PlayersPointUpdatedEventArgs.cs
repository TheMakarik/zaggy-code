namespace ZaggyCode.Core.Game.EventArgs;

public class PlayersPointUpdatedEventArgs : System.EventArgs
{
    public required int NewX { get; set; }
    public required int NewY { get; set; }
}