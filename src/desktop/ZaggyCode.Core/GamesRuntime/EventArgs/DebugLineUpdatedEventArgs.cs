namespace ZaggyCode.Core.GamesRuntime.EventArgs;

public sealed class DebugLineUpdatedEventArgs : System.EventArgs
{
    public int LineNumber { get; init; }
}
