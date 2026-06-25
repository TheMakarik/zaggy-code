namespace ZaggyCode.Languages.EventArgs;

public sealed class DebugLineUpdatedEventArgs : System.EventArgs
{
    public int LineNumber { get; init; }
}