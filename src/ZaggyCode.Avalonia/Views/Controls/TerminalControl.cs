using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using XTerm;
using XTerm.Buffer;
using XTerm.Common;
using XTerm.Input;
using AvaloniaKey = Avalonia.Input.Key;
using AvaloniaKeyEventArgs = Avalonia.Input.KeyEventArgs;
using AvaloniaKeyModifiers = Avalonia.Input.KeyModifiers;
using AvaloniaMouseButton = Avalonia.Input.MouseButton;
using AvaloniaPointerEventArgs = Avalonia.Input.PointerEventArgs;
using AvaloniaPointerPressedEventArgs = Avalonia.Input.PointerPressedEventArgs;
using AvaloniaPointerReleasedEventArgs = Avalonia.Input.PointerReleasedEventArgs;
using AvaloniaPointerUpdateKind = Avalonia.Input.PointerUpdateKind;
using AvaloniaPointerWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using AvaloniaTextInputEventArgs = Avalonia.Input.TextInputEventArgs;
using FocusChangedEventArgs = Avalonia.Input.FocusChangedEventArgs;
using XKey = XTerm.Input.Key;
using XKeyModifiers = XTerm.Input.KeyModifiers;

namespace ZaggyCode.Avalonia.Views.Controls;

public class TerminalControl : TemplatedControl, IDisposable
{
    private readonly Terminal _xTermDotNetTerminal;
    private readonly DispatcherTimer _blinkTimer;
    private readonly List<Color> _palette = new(256);

    private bool _cursorVisible = true;
    private bool _isFocused;
    private Typeface _typeface;
    private double _fontSize = 14;
    private Size _cellSize;

    public static readonly StyledProperty<double> TerminalFontSizeProperty =
        AvaloniaProperty.Register<TerminalControl, double>(nameof(TerminalFontSize), 14);

    public static readonly StyledProperty<string> TerminalFontFamilyProperty =
        AvaloniaProperty.Register<TerminalControl, string>(nameof(TerminalFontFamily), "Consolas");

    public TerminalControl()
    {
        _xTermDotNetTerminal = new Terminal(new XTerm.Options.TerminalOptions
        {
            Cols = 80,
            Rows = 24,
            Scrollback = 1000,
            CursorStyle = CursorStyle.Block,
            CursorBlink = true
        });

        _xTermDotNetTerminal.LineFed += (_, _) => InvalidateVisual();
        _xTermDotNetTerminal.BufferChanged += (_, _) => InvalidateVisual();
        _xTermDotNetTerminal.Scrolled += (_, _) => InvalidateVisual();
        _xTermDotNetTerminal.CursorStyleChanged += (_, _) => InvalidateVisual();

        Focusable = true;
        Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));

        _blinkTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        _blinkTimer.Tick += (_, _) =>
        {
            _cursorVisible = !_cursorVisible;
            InvalidateVisual();
        };

        InitializePalette();
        UpdateTypeface();
    }

    public Terminal XTermDotNetTerminal => _xTermDotNetTerminal;

    public double TerminalFontSize
    {
        get => GetValue(TerminalFontSizeProperty);
        set => SetValue(TerminalFontSizeProperty, value);
    }

    public string TerminalFontFamily
    {
        get => GetValue(TerminalFontFamilyProperty);
        set => SetValue(TerminalFontFamilyProperty, value);
    }

    public event EventHandler<string>? TerminalInput;

    public void Write(string text)
    {
        _xTermDotNetTerminal.Write(text);
        InvalidateVisual();
    }

    public void WriteLine(string text)
    {
        _xTermDotNetTerminal.WriteLine(text);
        InvalidateVisual();
    }

    protected virtual void OnTerminalInput(string input)
    {
        TerminalInput?.Invoke(this, input);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != TerminalFontSizeProperty && change.Property != TerminalFontFamilyProperty) 
            return;
        
        UpdateTypeface();
        ResizeTerminal(Bounds.Size);
        InvalidateVisual();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_xTermDotNetTerminal.Options.CursorBlink)
            _blinkTimer.Start();
        
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _blinkTimer.Stop();
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        _isFocused = true;
        _cursorVisible = true;
        InvalidateVisual();
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        _isFocused = false;
        _cursorVisible = true;
        InvalidateVisual();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ResizeTerminal(e.NewSize);
    }

    protected override void OnKeyDown(AvaloniaKeyEventArgs e)
    {
        base.OnKeyDown(e);

        var modifiers = GetKeyModifiers(e.KeyModifiers);
        string? input = null;

        if (e.Key is >= AvaloniaKey.A and <= AvaloniaKey.Z && modifiers.HasFlag(XKeyModifiers.Control))
        {
            char c = (char)('a' + (e.Key - AvaloniaKey.A));
            input = _xTermDotNetTerminal.GenerateCharInput(c, modifiers);
        }
        else
        {
            input = e.Key switch
            {
                AvaloniaKey.Enter or AvaloniaKey.Return => _xTermDotNetTerminal.GenerateKeyInput(XKey.Enter, modifiers),
                AvaloniaKey.Tab => _xTermDotNetTerminal.GenerateKeyInput(XKey.Tab, modifiers),
                AvaloniaKey.Back => _xTermDotNetTerminal.GenerateKeyInput(XKey.Backspace, modifiers),
                AvaloniaKey.Delete => _xTermDotNetTerminal.GenerateKeyInput(XKey.Delete, modifiers),
                AvaloniaKey.Escape => _xTermDotNetTerminal.GenerateKeyInput(XKey.Escape, modifiers),
                AvaloniaKey.Up => _xTermDotNetTerminal.GenerateKeyInput(XKey.UpArrow, modifiers),
                AvaloniaKey.Down => _xTermDotNetTerminal.GenerateKeyInput(XKey.DownArrow, modifiers),
                AvaloniaKey.Left => _xTermDotNetTerminal.GenerateKeyInput(XKey.LeftArrow, modifiers),
                AvaloniaKey.Right => _xTermDotNetTerminal.GenerateKeyInput(XKey.RightArrow, modifiers),
                AvaloniaKey.Home => _xTermDotNetTerminal.GenerateKeyInput(XKey.Home, modifiers),
                AvaloniaKey.End => _xTermDotNetTerminal.GenerateKeyInput(XKey.End, modifiers),
                AvaloniaKey.PageUp => _xTermDotNetTerminal.GenerateKeyInput(XKey.PageUp, modifiers),
                AvaloniaKey.PageDown => _xTermDotNetTerminal.GenerateKeyInput(XKey.PageDown, modifiers),
                AvaloniaKey.Insert => _xTermDotNetTerminal.GenerateKeyInput(XKey.Insert, modifiers),
                AvaloniaKey.F1 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F1, modifiers),
                AvaloniaKey.F2 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F2, modifiers),
                AvaloniaKey.F3 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F3, modifiers),
                AvaloniaKey.F4 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F4, modifiers),
                AvaloniaKey.F5 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F5, modifiers),
                AvaloniaKey.F6 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F6, modifiers),
                AvaloniaKey.F7 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F7, modifiers),
                AvaloniaKey.F8 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F8, modifiers),
                AvaloniaKey.F9 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F9, modifiers),
                AvaloniaKey.F10 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F10, modifiers),
                AvaloniaKey.F11 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F11, modifiers),
                AvaloniaKey.F12 => _xTermDotNetTerminal.GenerateKeyInput(XKey.F12, modifiers),
                AvaloniaKey.Space => _xTermDotNetTerminal.GenerateCharInput(' ', modifiers),
                _ => null
            };
        }

        if (!string.IsNullOrEmpty(input))
        {
            OnTerminalInput(input);
            e.Handled = true;
        }
    }

    protected override void OnTextInput(AvaloniaTextInputEventArgs e)
    {
        base.OnTextInput(e);

        foreach (char c in e.Text ?? string.Empty)
        {
            string input = _xTermDotNetTerminal.GenerateCharInput(c, XKeyModifiers.None);
            if (!string.IsNullOrEmpty(input))
            {
                OnTerminalInput(input);
            }
        }

        e.Handled = true;
    }

    protected override void OnPointerPressed(AvaloniaPointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();

        var point = e.GetPosition(this);
        var (col, row) = HitTest(point);
        var button = GetXTermMouseButton(e.GetCurrentPoint(this).Properties.PointerUpdateKind);
        string input = _xTermDotNetTerminal.GenerateMouseEvent(button, col + 1, row + 1, MouseEventType.Down, GetKeyModifiers(e.KeyModifiers));

        if (!string.IsNullOrEmpty(input))
        {
            OnTerminalInput(input);
        }
    }

    protected override void OnPointerReleased(AvaloniaPointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        var point = e.GetPosition(this);
        var (col, row) = HitTest(point);
        var button = GetXTermMouseButtonFromAvalonia(e.InitialPressMouseButton);
        string input = _xTermDotNetTerminal.GenerateMouseEvent(button, col + 1, row + 1, MouseEventType.Up, GetKeyModifiers(e.KeyModifiers));

        if (!string.IsNullOrEmpty(input))
        {
            OnTerminalInput(input);
        }
    }

    protected override void OnPointerMoved(AvaloniaPointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed && !properties.IsRightButtonPressed)
        {
            return;
        }

        var (col, row) = HitTest(point);
        string input = _xTermDotNetTerminal.GenerateMouseEvent(MouseButton.Left, col + 1, row + 1, MouseEventType.Drag, GetKeyModifiers(e.KeyModifiers));

        if (!string.IsNullOrEmpty(input))
        {
            OnTerminalInput(input);
        }
    }

    protected override void OnPointerWheelChanged(AvaloniaPointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var point = e.GetPosition(this);
        var (col, row) = HitTest(point);
        var button = e.Delta.Y > 0 ? MouseButton.WheelUp : MouseButton.WheelDown;
        var modifiers = GetKeyModifiers(e.KeyModifiers);
        string input = _xTermDotNetTerminal.GenerateMouseEvent(button, col + 1, row + 1, MouseEventType.Down, modifiers);

        if (!string.IsNullOrEmpty(input))
        {
            OnTerminalInput(input);
        }
        else if (!_xTermDotNetTerminal.IsAlternateBufferActive)
        {
            _xTermDotNetTerminal.ScrollLines(e.Delta.Y > 0 ? -3 : 3);
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var buffer = _xTermDotNetTerminal.Buffer;

        context.FillRectangle(Background ?? Brushes.Transparent, bounds);

        for (int row = 0; row < _xTermDotNetTerminal.Rows; row++)
        {
            int lineIndex = buffer.YDisp + row;
            if (lineIndex < 0 || lineIndex >= buffer.Lines.Length)
            {
                continue;
            }

            var line = buffer.Lines[lineIndex];
            if (line == null)
            {
                continue;
            }

            for (int col = 0; col < _xTermDotNetTerminal.Cols; col++)
            {
                var cell = line[col];
                if (cell.Width == 0)
                {
                    continue;
                }

                double x = col * _cellSize.Width;
                double y = row * _cellSize.Height;
                double cellWidth = cell.Width * _cellSize.Width;

                var bg = GetBackgroundBrush(cell.Attributes);
                if (bg != null)
                {
                    context.FillRectangle(bg, new Rect(x, y, cellWidth, _cellSize.Height));
                }

                if (!string.IsNullOrEmpty(cell.Content) && cell.Content != " ")
                {
                    var fg = GetForegroundBrush(cell.Attributes);
                    var weight = cell.Attributes.IsBold() ? FontWeight.Bold : FontWeight.Normal;
                    var text = CreateFormattedText(cell.Content, fg, weight);
                    context.DrawText(text, new Point(x, y));
                }
            }
        }

        RenderCursor(context, buffer);
    }

    public void Dispose()
    {
        _blinkTimer.Stop();
        _xTermDotNetTerminal.Dispose();
    }

    private void InitializePalette()
    {
        var standardColors = new[]
        {
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(205, 0, 0),
            Color.FromRgb(0, 205, 0),
            Color.FromRgb(205, 205, 0),
            Color.FromRgb(0, 0, 238),
            Color.FromRgb(205, 0, 205),
            Color.FromRgb(0, 205, 205),
            Color.FromRgb(229, 229, 229),
            Color.FromRgb(127, 127, 127),
            Color.FromRgb(255, 0, 0),
            Color.FromRgb(0, 255, 0),
            Color.FromRgb(255, 255, 0),
            Color.FromRgb(92, 92, 255),
            Color.FromRgb(255, 0, 255),
            Color.FromRgb(0, 255, 255),
            Color.FromRgb(255, 255, 255)
        };

        _palette.AddRange(standardColors);

        for (int r = 0; r < 6; r++)
        {
            for (int g = 0; g < 6; g++)
            {
                for (int b = 0; b < 6; b++)
                {
                    byte rr = r == 0 ? (byte)0 : (byte)(95 + (r - 1) * 40);
                    byte gg = g == 0 ? (byte)0 : (byte)(95 + (g - 1) * 40);
                    byte bb = b == 0 ? (byte)0 : (byte)(95 + (b - 1) * 40);
                    _palette.Add(Color.FromRgb(rr, gg, bb));
                }
            }
        }

        for (int i = 0; i < 24; i++)
        {
            byte c = (byte)(8 + i * 10);
            _palette.Add(Color.FromRgb(c, c, c));
        }
    }

    private void UpdateTypeface()
    {
        _fontSize = TerminalFontSize;
        _typeface = new Typeface(new FontFamily(TerminalFontFamily), FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);
        MeasureCellSize();
    }

    private void MeasureCellSize()
    {
        var text = CreateFormattedText("W", Brushes.Black, FontWeight.Normal);
        _cellSize = new Size(text.Width, text.Height);
    }

    private void ResizeTerminal(Size size)
    {
        if (_cellSize.Width <= 0 || _cellSize.Height <= 0)
        {
            return;
        }

        int cols = (int)(size.Width / _cellSize.Width);
        int rows = (int)(size.Height / _cellSize.Height);

        if (cols > 0 && rows > 0 && (cols != _xTermDotNetTerminal.Cols || rows != _xTermDotNetTerminal.Rows))
        {
            _xTermDotNetTerminal.Resize(cols, rows);
            InvalidateVisual();
        }
    }

    private (int col, int row) HitTest(Point point)
    {
        int col = Math.Max(0, (int)(point.X / _cellSize.Width));
        int row = Math.Max(0, (int)(point.Y / _cellSize.Height));
        return (col, row);
    }

    private static XKeyModifiers GetKeyModifiers(AvaloniaKeyModifiers modifiers)
    {
        XKeyModifiers result = XKeyModifiers.None;
        if (modifiers.HasFlag(AvaloniaKeyModifiers.Control))
        {
            result |= XKeyModifiers.Control;
        }

        if (modifiers.HasFlag(AvaloniaKeyModifiers.Shift))
        {
            result |= XKeyModifiers.Shift;
        }

        if (modifiers.HasFlag(AvaloniaKeyModifiers.Alt))
        {
            result |= XKeyModifiers.Alt;
        }

        return result;
    }

    private static XTerm.Input.MouseButton GetXTermMouseButton(AvaloniaPointerUpdateKind kind)
    {
        return kind switch
        {
            AvaloniaPointerUpdateKind.LeftButtonPressed or AvaloniaPointerUpdateKind.LeftButtonReleased => MouseButton.Left,
            AvaloniaPointerUpdateKind.RightButtonPressed or AvaloniaPointerUpdateKind.RightButtonReleased => MouseButton.Right,
            AvaloniaPointerUpdateKind.MiddleButtonPressed or AvaloniaPointerUpdateKind.MiddleButtonReleased => MouseButton.Middle,
            _ => MouseButton.None
        };
    }

    private static XTerm.Input.MouseButton GetXTermMouseButtonFromAvalonia(AvaloniaMouseButton button)
    {
        return button switch
        {
            AvaloniaMouseButton.Left => MouseButton.Left,
            AvaloniaMouseButton.Right => MouseButton.Right,
            AvaloniaMouseButton.Middle => MouseButton.Middle,
            _ => MouseButton.None
        };
    }

    private FormattedText CreateFormattedText(string text, IBrush brush, FontWeight weight)
    {
        var typeface = new Typeface(_typeface.FontFamily, _typeface.Style, weight, _typeface.Stretch);
        return new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, _fontSize, brush);
    }

    private IBrush GetForegroundBrush(AttributeData attributes)
    {
        int mode = attributes.GetFgColorMode();
        int value = attributes.GetFgColor();

        if (mode == 0 && value == 256)
        {
            return Foreground ?? Brushes.White;
        }

        return new SolidColorBrush(GetColor(mode, value));
    }

    private IBrush? GetBackgroundBrush(AttributeData attributes)
    {
        int mode = attributes.GetBgColorMode();
        int value = attributes.GetBgColor();

        if (mode == 0 && value == 257)
        {
            return null;
        }

        return new SolidColorBrush(GetColor(mode, value));
    }

    private Color GetColor(int mode, int value)
    {
        if (mode == 0 && value >= 0 && value < _palette.Count)
        {
            return _palette[value];
        }

        if (mode == 1)
        {
            byte r = (byte)((value >> 16) & 0xFF);
            byte g = (byte)((value >> 8) & 0xFF);
            byte b = (byte)(value & 0xFF);
            return Color.FromRgb(r, g, b);
        }

        return Colors.Black;
    }

    private void RenderCursor(DrawingContext context, TerminalBuffer buffer)
    {
        if (!_xTermDotNetTerminal.CursorVisible || !_isFocused)
        {
            return;
        }

        if (_xTermDotNetTerminal.Options.CursorBlink && !_cursorVisible)
        {
            return;
        }

        double x = buffer.X * _cellSize.Width;
        double y = buffer.Y * _cellSize.Height;
        var cursorBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));

        switch (_xTermDotNetTerminal.Options.CursorStyle)
        {
            case CursorStyle.Block:
                context.FillRectangle(cursorBrush, new Rect(x, y, _cellSize.Width, _cellSize.Height));
                break;
            case CursorStyle.Underline:
                context.FillRectangle(cursorBrush, new Rect(x, y + _cellSize.Height - 2, _cellSize.Width, 2));
                break;
            case CursorStyle.Bar:
                context.FillRectangle(cursorBrush, new Rect(x, y, 2, _cellSize.Height));
                break;
        }
    }
}
