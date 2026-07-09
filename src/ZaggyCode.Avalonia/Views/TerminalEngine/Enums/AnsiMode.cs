namespace ZaggyCode.Avalonia.Views.TerminalEngine.Enums;


public enum AnsiMode
{
    ShowCursor,
    HideCursor,
    LineFeed,
    NewLine,
    CursorKeyToCursor,
    CursorKeyToApplication,
    ANSI,
    VT52,
    Columns80,
    Columns132,
    JumpScrolling,
    SmoothScrolling,
    NormalVideo,
    ReverseVideo,
    OriginIsAbsolute,
    OriginIsRelative,
    LineWrap,
    DisableLineWrap,
    AutoRepeat,
    DisableAutoRepeat,
    Interlacing,
    DisableInterlacing,
    NumericKeypad,
    AlternateKeypad,
}
