using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using ZaggyCode.Avalonia.Views.TerminalEngine;
using ZaggyCode.Avalonia.Views.TerminalEngine.Session;

namespace ZaggyCode.Avalonia.Views.Controls;

public class TerminalControl : TemplatedControl, IDisposable
{
    private static readonly Color defaultForeground = Colors.White;
    private static readonly Color defaultBackground = Colors.Black;

    private readonly DrawingGroup _rootDrawingGroup = new DrawingGroup();
    private readonly DrawingGroup _cursorVisual = new DrawingGroup();
    private readonly List<DrawingGroup> _lineDrawings = [];

    private readonly Lock _renderLock;
    private readonly DispatcherTimer _renderTimer;
    private readonly DispatcherTimer _blinkTimer;
    private readonly ITerminalSession _session;

    private bool cursorState = true;
    private bool _cursorVisible = true;
    private bool needsFullInvalidation = true;

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

    public TerminalScreenBuffer? CurrentBuffer
    {
        get => GetValue(CurrentBufferProperty);
        set => SetValue(CurrentBufferProperty, value);
    }

    public bool CursorVisible
    {
        get => GetValue(CursorVisibleProperty);
        set => SetValue(CursorVisibleProperty, value);
    }

    public ITerminalSession Session
    {
        get => _session;
    }

    public Point CursorPosition
    {
        get;
        private set;
    }

    public TerminalControl()
    {
        ITerminalSession session = new DummyTerminalSession();
        session.Resize(512, 10240);

        _session = session;
        _renderLock = new Lock();
        _renderTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Background, Dispatcher.CurrentDispatcher, DispatcherRenderHandler);
        _blinkTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(530), DispatcherPriority.Background, Dispatcher.CurrentDispatcher, DispatcherBlinkHandler);

        CurrentBuffer = _session.Buffer;
        _rootDrawingGroup.Children.Add(_cursorVisual);
        _session.BufferUpdated += OnSessionBufferUpdated;
        _renderTimer.Start();
    }

    private void OnSessionBufferUpdated(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            needsFullInvalidation = true;
            InvalidateVisual();
        });
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CurrentBufferProperty ||
            change.Property == TerminalFontSizeProperty ||
            change.Property == TerminalFontFamilyProperty)
        {
            needsFullInvalidation = true;
            InvalidateVisual();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
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
        _cursorVisible = true;
        InvalidateVisual();
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        _cursorVisible = true;
        InvalidateVisual();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (needsFullInvalidation)
        {
            RenderFullBuffer();
            needsFullInvalidation = false;
        }

        lock (_renderLock)
        {
            if (Background is not null)
                context.DrawRectangle(Background, null, new Rect(Bounds.Size));

            _rootDrawingGroup.Draw(context);
        }
    }

    private void DispatcherRenderHandler(object? sender, EventArgs e)
    {
        if (CurrentBuffer == null)
            return;

        if (CurrentBuffer.HasDirtyLines())
        {
            for (int i = 0; i < CurrentBuffer.RowsCount; i++)
            {
                if (CurrentBuffer.IsLineDirty(i))
                {
                    InvalidateRow(i);
                    CurrentBuffer.MarkLineClean(i);
                }
            }
        }
    }

    private void DispatcherBlinkHandler(object? sender, EventArgs e)
    {
        if (!CursorVisible)
        {
            cursorState = false;
            RenderCursorFromBuffer();
            return;
        }

        cursorState = !cursorState;
        RenderCursorFromBuffer();
    }

    void RenderFullBuffer()
    {
        if (CurrentBuffer == null)
            return;

        lock (_renderLock)
        {
            CorrectVisualsCountForBuffer();
            for (int y = 0; y < CurrentBuffer.GridSize.Height && y < _lineDrawings.Count; y++)
                RenderRowFromBuffer(y);

            UpdateCursorPositionFromBuffer();
        }
    }

    private void CorrectVisualsCountForBuffer()
    {
        if (CurrentBuffer == null)
            return;

        int targetRows = Math.Min(CurrentBuffer.RowsCount, (int)CurrentBuffer.GridSize.Height);
        int currentRows = _lineDrawings.Count;
        int difference = targetRows - currentRows;

        if (difference == 0)
            return;

        if (difference > 0)
        {
            for (int i = 0; i < difference; i++)
            {
                DrawingGroup newVisual = new DrawingGroup();
                _lineDrawings.Add(newVisual);
                _rootDrawingGroup.Children.Insert(_rootDrawingGroup.Children.Count - 1, newVisual);
            }
        }
        else if (difference < 0)
        {
            for (int i = 0; i < -difference; i++)
            {
                _lineDrawings.RemoveAt(_lineDrawings.Count - 1);
                _rootDrawingGroup.Children.RemoveAt(_rootDrawingGroup.Children.Count - 2);
            }
        }
    }

    private void InvalidateRow(int row)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => InvalidateRow(row));
            return;
        }

        if (CurrentBuffer == null || row < 0 || row >= CurrentBuffer.RowsCount)
            return;

        lock (_renderLock)
        {
            CorrectVisualsCountForBuffer();
            if (row < _lineDrawings.Count)
            {
                RenderRowFromBuffer(row);
            }
        }
    }

    private void RenderRowFromBuffer(int row)
    {
        if (CurrentBuffer == null || row < 0 || row >= CurrentBuffer.RowsCount || row >= _lineDrawings.Count)
            return;

        try
        {
            Size cellSize = GetCellSize();
            double verticalOffset = row * cellSize.Height;

            DrawingGroup visual = _lineDrawings[row];
            visual.Transform = new TranslateTransform(0, verticalOffset);

            Span<TerminalCellInfo> rowSpan = CurrentBuffer.GetRow(row);
            RenderRowFromTerminalBuffer(visual, rowSpan, cellSize);
        }
        catch
        {
            // fucked up somewhere
            _ = 0xBAD + 0xC0DE;
        }
    }

    private void RenderRowFromTerminalBuffer(DrawingGroup visual, Span<TerminalCellInfo> rowSpan, Size cellSize)
    {
        using DrawingContext dc = visual.Open();
        double horizontalOffset = 0;

        Span<char> textSpan = rowSpan.Length > 1024
            ? new char[rowSpan.Length]
            : stackalloc char[rowSpan.Length];

        for (int x = 0; x < rowSpan.Length;)
        {
            try
            {
                Span<TerminalCellInfo> remainingSlice = rowSpan.Slice(x);
                int runLength = TakeWhileSameAttributes(remainingSlice);
                x += runLength;

                Span<TerminalCellInfo> runSpan = remainingSlice.Slice(0, runLength);

                for (int i = 0; i < runLength; i++)
                    textSpan[i] = runSpan[i].Character;

                TerminalCellInfo firstCell = runSpan[0];

                // Apply bold if needed
                FontWeight weight = firstCell.Bold ? FontWeight.Bold : FontWeight.Normal;
                Typeface runTypeface = new Typeface(new FontFamily(TerminalFontFamily), FontStyle.Normal, weight, FontStretch.Normal);

                FormattedText formatted = new FormattedText(
                    new string(textSpan.Slice(0, runLength)),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    runTypeface,
                    TerminalFontSize,
                    firstCell.Foreground == defaultForeground ? Foreground : new SolidColorBrush(firstCell.Foreground));

                if (firstCell.Background != defaultBackground)
                {
                    dc.DrawRectangle(
                        new SolidColorBrush(firstCell.Background), null,
                        new Rect(horizontalOffset, 0, formatted.WidthIncludingTrailingWhitespace, formatted.Height));
                }

                dc.DrawText(formatted, new Point(horizontalOffset, 0));
                horizontalOffset += formatted.WidthIncludingTrailingWhitespace;
            }
            catch
            {
                // fucked up somewhere
                _ = 0xBAD + 0xC0DE;
            }
        }
    }

    private static int TakeWhileSameAttributes(ReadOnlySpan<TerminalCellInfo> span)
    {
        if (span.IsEmpty)
            return 0;

        TerminalCellInfo first = span[0];
        int length = span.Length;

        for (int i = 1; i < length; i++)
        {
            TerminalCellInfo current = span[i];
            if (first != current)
                return i;
        }

        return span.Length;
    }

    private void UpdateCursorPositionFromBuffer()
    {
        if (CurrentBuffer == null)
            return;

        if (_session.Decoder is TerminalDecoder decoder)
            CursorPosition = decoder.CursorPosition;

        Size cellSize = GetCellSize();
        _cursorVisual.Transform = new TranslateTransform(CursorPosition.X * cellSize.Width, CursorPosition.Y * cellSize.Height);
    }

    private void RenderCursorFromBuffer()
    {
        if (CurrentBuffer == null)
            return;

        try
        {
            UpdateCursorPositionFromBuffer();

            using DrawingContext dc = _cursorVisual.Open();
            IBrush? background = cursorState ? Foreground : Brushes.Transparent;
            dc.DrawRectangle(background, new Pen(), new Rect(0, 0, 1, GetCellSize().Height));
        }
        catch (Exception exc)
        {
            Debug.WriteLine(exc);
        }
    }

    public Size GetCellSize()
    {
        FormattedText formattedText = Format("M", Brushes.Black);
        return new Size(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
    }

    private FormattedText Format(string text, IBrush foreground)
    {
        Typeface typeface = new Typeface(new FontFamily(TerminalFontFamily), FontStyle.Normal, FontWeight.Normal, FontStretch.Normal);
        return new FormattedText(
            text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            typeface, TerminalFontSize, foreground);
    }

    public void Dispose()
    {
        _renderTimer.Stop();
        _blinkTimer.Stop();
        _session.BufferUpdated -= OnSessionBufferUpdated;
        _session.Dispose();
    }

    public static readonly StyledProperty<TerminalScreenBuffer?> CurrentBufferProperty =
        AvaloniaProperty.Register<TerminalControl, TerminalScreenBuffer?>(nameof(CurrentBuffer), null);

    public static readonly StyledProperty<bool> CursorVisibleProperty =
        AvaloniaProperty.Register<TerminalControl, bool>(nameof(CursorVisible), true);

    public static readonly StyledProperty<double> TerminalFontSizeProperty =
        AvaloniaProperty.Register<TerminalControl, double>(nameof(TerminalFontSize), 14);

    public static readonly StyledProperty<string> TerminalFontFamilyProperty =
        AvaloniaProperty.Register<TerminalControl, string>(nameof(TerminalFontFamily), "Consolas");
}
