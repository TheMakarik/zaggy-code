using System;
using System.Drawing;
using ZaggyCode.Avalonia.Views.TerminalEngine.Enums;

namespace ZaggyCode.Avalonia.Views.TerminalEngine.Interfaces;

public interface ITerminalScreenView
{
    Point CursorPosition { get; }

    void Characters(ITerminalDecoder sender, ReadOnlySpan<char> chars);
    void SaveCursor(ITerminalDecoder sernder);
    void RestoreCursor(ITerminalDecoder sender);
    Size GetSize(ITerminalDecoder sender);
    void MoveCursor(ITerminalDecoder sender, Direction _direction, int amount);
    void MoveCursorToBeginningOfLineBelow(ITerminalDecoder sender, int lineNumberRelativeToCurrentLine);
    void MoveCursorToBeginningOfLineAbove(ITerminalDecoder sender, int lineNumberRelativeToCurrentLine);
    void MoveCursorToColumn(ITerminalDecoder sender, int columnNumber);
    void MoveCursorTo(ITerminalDecoder sender, Point position);
    void ClearScreen(ITerminalDecoder sender, ClearDirection direction);
    void ClearLine(ITerminalDecoder sender, ClearDirection _direction);
    void ScrollPageUpwards(ITerminalDecoder sender, int linesToScroll);
    void ScrollPageDownwards(ITerminalDecoder sender, int linesToScroll);
    Point GetCursorPosition(ITerminalDecoder sender);
    void SetGraphicRendition(ITerminalDecoder sender, GraphicRendition[] commands);
    void ModeChanged(ITerminalDecoder sender, AnsiMode mode);
}
