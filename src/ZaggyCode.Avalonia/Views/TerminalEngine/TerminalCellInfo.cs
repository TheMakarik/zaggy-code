namespace ZaggyCode.Avalonia.Views.TerminalEngine;

[DebuggerDisplay("'{Character}'")]
public struct TerminalCellInfo() : IEquatable<TerminalCellInfo>
{
    public char Character { get; set; } = ' ';
    public bool Bold { get; set; }
    public bool Faint { get; set; }
    public bool Italic { get; set; }
    public Underline Underline { get; set; }
    public Blink Blink { get; set; }
    public bool Conceal { get; set; }
    public Color Foreground { get; set; } = Colors.White;
    public Color Background { get; set; } = Colors.Black;

    public void Reset()
    {
        Bold = false;
        Faint = false;
        Italic = false;
        Underline = Underline.None;
        Blink = Blink.None;
        Conceal = false;
        Foreground = Colors.White;
        Background = Colors.Black;
    }

    public readonly bool Equals(TerminalCellInfo other)
    {
        return Foreground == other.Foreground
               && Background == other.Background
               && Bold == other.Bold
               && Faint == other.Faint
               && Italic == other.Italic
               && Underline == other.Underline
               && Blink == other.Blink
               && Conceal == other.Conceal;
    }

    public override readonly bool Equals(object? obj)
        => obj is TerminalCellInfo info && Equals(info);

    public static bool operator ==(TerminalCellInfo left, TerminalCellInfo right)
        => left.Equals(right);

    public static bool operator !=(TerminalCellInfo left, TerminalCellInfo right)
        => !(left == right);

    public override int GetHashCode()
        => Character.GetHashCode();
}
