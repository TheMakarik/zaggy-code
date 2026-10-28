namespace ZaggyCode.Core.GamesRuntime.EventArgs;

public sealed class CodeErrorOccurredEventArgs : System.EventArgs
{
    public required string Text { get; set; }
}
