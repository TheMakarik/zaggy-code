namespace ZaggyCode.Avalonia.Views.Controls;

public sealed class ThemeSchemePreview : Control
{
    public static readonly StyledProperty<IBrush?> TitleBarBrushProperty =
        AvaloniaProperty.Register<ThemeSchemePreview, IBrush?>(nameof(TitleBarBrush));

    public static readonly StyledProperty<IBrush?> WindowBackgroundBrushProperty =
        AvaloniaProperty.Register<ThemeSchemePreview, IBrush?>(nameof(WindowBackgroundBrush));

    public static readonly StyledProperty<IBrush?> SidebarBrushProperty =
        AvaloniaProperty.Register<ThemeSchemePreview, IBrush?>(nameof(SidebarBrush));

    public static readonly StyledProperty<IBrush?> EditorBrushProperty =
        AvaloniaProperty.Register<ThemeSchemePreview, IBrush?>(nameof(EditorBrush));

    public static readonly StyledProperty<IBrush?> TerminalBrushProperty =
        AvaloniaProperty.Register<ThemeSchemePreview, IBrush?>(nameof(TerminalBrush));

    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<ThemeSchemePreview, IBrush?>(nameof(OutlineBrush));

    public static readonly StyledProperty<IBrush?> ContentForegroundBrushProperty =
        AvaloniaProperty.Register<ThemeSchemePreview, IBrush?>(nameof(ContentForegroundBrush));

    static ThemeSchemePreview()
    {
        AffectsRender<ThemeSchemePreview>(TitleBarBrushProperty);
        AffectsRender<ThemeSchemePreview>(WindowBackgroundBrushProperty);
        AffectsRender<ThemeSchemePreview>(SidebarBrushProperty);
        AffectsRender<ThemeSchemePreview>(EditorBrushProperty);
        AffectsRender<ThemeSchemePreview>(TerminalBrushProperty);
        AffectsRender<ThemeSchemePreview>(OutlineBrushProperty);
        AffectsRender<ThemeSchemePreview>(ContentForegroundBrushProperty);
    }

    public IBrush? TitleBarBrush
    {
        get => GetValue(TitleBarBrushProperty);
        set => SetValue(TitleBarBrushProperty, value);
    }

    public IBrush? WindowBackgroundBrush
    {
        get => GetValue(WindowBackgroundBrushProperty);
        set => SetValue(WindowBackgroundBrushProperty, value);
    }

    public IBrush? SidebarBrush
    {
        get => GetValue(SidebarBrushProperty);
        set => SetValue(SidebarBrushProperty, value);
    }

    public IBrush? EditorBrush
    {
        get => GetValue(EditorBrushProperty);
        set => SetValue(EditorBrushProperty, value);
    }

    public IBrush? TerminalBrush
    {
        get => GetValue(TerminalBrushProperty);
        set => SetValue(TerminalBrushProperty, value);
    }

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public IBrush? ContentForegroundBrush
    {
        get => GetValue(ContentForegroundBrushProperty);
        set => SetValue(ContentForegroundBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        var windowRect = new Rect(bounds.Size);
        var cornerRadius = Math.Min(5, bounds.Width * 0.06);
        var outlinePen = new Pen(OutlineBrush, 1);

        context.DrawRectangle(
            WindowBackgroundBrush ?? Brushes.Transparent,
            outlinePen,
            new RoundedRect(windowRect, cornerRadius));

        var titleBarRect = new Rect(0, 0, bounds.Width, bounds.Height * 0.17);
        context.DrawRectangle(TitleBarBrush ?? Brushes.Transparent, null,
            new RoundedRect(titleBarRect, new CornerRadius(cornerRadius, cornerRadius, 0, 0)));

        var bodyTop = titleBarRect.Height + 1;
        var sidebarWidth = bounds.Width * 0.26;
        var terminalTop = bounds.Height * 0.62;

        context.FillRectangle(SidebarBrush ?? Brushes.Transparent,
            new Rect(1, bodyTop, sidebarWidth - 2, bounds.Height - bodyTop - 1));
        context.FillRectangle(EditorBrush ?? Brushes.Transparent,
            new Rect(sidebarWidth, bodyTop, bounds.Width - sidebarWidth - 1, terminalTop - bodyTop));
        context.FillRectangle(TerminalBrush ?? Brushes.Transparent,
            new Rect(sidebarWidth, terminalTop + 1, bounds.Width - sidebarWidth - 1, bounds.Height - terminalTop - 2));

        DrawContentAccents(context, sidebarWidth, bodyTop, terminalTop);
    }

    private void DrawContentAccents(DrawingContext context, double sidebarWidth, double bodyTop, double terminalTop)
    {
        var accent = ContentForegroundBrush;
        if (accent is null)
            return;

        var lineStep = Bounds.Height * 0.075;
        var sidebarLines = new[] { 0.12, 0.32, 0.52 };
        foreach (var offset in sidebarLines)
            context.FillRectangle(accent,
                new Rect(Bounds.Width * 0.05, bodyTop + Bounds.Height * offset, sidebarWidth * 0.6, Math.Max(1.5, lineStep * 0.28)));

        var editorLines = new[] { 0.10, 0.22, 0.34 };
        foreach (var offset in editorLines)
        {
            var widthFactor = offset == editorLines[1] ? 0.45 : 0.65;
            context.FillRectangle(accent,
                new Rect(Bounds.Width * 0.32, bodyTop + Bounds.Height * offset, Bounds.Width * widthFactor, Math.Max(1.5, lineStep * 0.28)));
        }

        context.FillRectangle(accent,
            new Rect(Bounds.Width * 0.32, terminalTop + Bounds.Height * 0.14, Bounds.Width * 0.4, Math.Max(1.5, lineStep * 0.28)));
    }
}
