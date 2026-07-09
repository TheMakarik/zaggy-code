namespace ZaggyCode.Core.Languages.EventArgs;

public sealed class DebugLineUpdatedEventArgs : System.EventArgs
{
    public int LineNumber { get; init; }
}