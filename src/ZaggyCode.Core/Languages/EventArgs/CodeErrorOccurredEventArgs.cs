namespace ZaggyCode.Core.Languages.EventArgs;

public sealed class CodeErrorOccurredEventArgs : System.EventArgs
{
    public required string Text { get; set; }
}