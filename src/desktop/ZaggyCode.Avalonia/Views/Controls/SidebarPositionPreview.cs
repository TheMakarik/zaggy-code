namespace ZaggyCode.Avalonia.Views.Controls;

public sealed class SidebarPositionPreview : Control
{
    public static readonly StyledProperty<SidebarPosition> PositionProperty =
        AvaloniaProperty.Register<SidebarPositionPreview, SidebarPosition>(nameof(Position));

    public static readonly StyledProperty<IBrush?> WindowBackgroundBrushProperty =
        AvaloniaProperty.Register<SidebarPositionPreview, IBrush?>(nameof(WindowBackgroundBrush));

    public static readonly StyledProperty<IBrush?> HeaderBrushProperty =
        AvaloniaProperty.Register<SidebarPositionPreview, IBrush?>(nameof(HeaderBrush));

    public static readonly StyledProperty<IBrush?> SidebarBrushProperty =
        AvaloniaProperty.Register<SidebarPositionPreview, IBrush?>(nameof(SidebarBrush));

    public static readonly StyledProperty<IBrush?> ContentBrushProperty =
        AvaloniaProperty.Register<SidebarPositionPreview, IBrush?>(nameof(ContentBrush));

    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<SidebarPositionPreview, IBrush?>(nameof(OutlineBrush));

    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<SidebarPositionPreview, IBrush?>(nameof(AccentBrush));

    static SidebarPositionPreview()
    {
        AffectsRender<SidebarPositionPreview>(
            PositionProperty,
            WindowBackgroundBrushProperty,
            HeaderBrushProperty,
            SidebarBrushProperty,
            ContentBrushProperty,
            OutlineBrushProperty,
            AccentBrushProperty);
    }

    public SidebarPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public IBrush? WindowBackgroundBrush
    {
        get => GetValue(WindowBackgroundBrushProperty);
        set => SetValue(WindowBackgroundBrushProperty, value);
    }

    public IBrush? HeaderBrush
    {
        get => GetValue(HeaderBrushProperty);
        set => SetValue(HeaderBrushProperty, value);
    }

    public IBrush? SidebarBrush
    {
        get => GetValue(SidebarBrushProperty);
        set => SetValue(SidebarBrushProperty, value);
    }

    public IBrush? ContentBrush
    {
        get => GetValue(ContentBrushProperty);
        set => SetValue(ContentBrushProperty, value);
    }

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
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

        var headerHeight = bounds.Height * 0.17;
        context.DrawRectangle(
            HeaderBrush ?? Brushes.Transparent,
            null,
            new RoundedRect(
                new Rect(0, 0, bounds.Width, headerHeight),
                new CornerRadius(cornerRadius, cornerRadius, 0, 0)));

        var bodyTop = headerHeight + 1;
        var bodyWidth = Math.Max(0, bounds.Width - 2);
        var bodyHeight = Math.Max(0, bounds.Height - bodyTop - 1);
        var sidebarThickness = Math.Max(0, Math.Min(bounds.Width, bounds.Height) * 0.26);
        var contentRect = new Rect(1, bodyTop, bodyWidth, bodyHeight);

        switch (Position)
        {
            case SidebarPosition.Left:
                context.FillRectangle(
                    SidebarBrush ?? Brushes.Transparent,
                    new Rect(1, bodyTop, sidebarThickness, bodyHeight));
                contentRect = new Rect(
                    1 + sidebarThickness,
                    bodyTop,
                    Math.Max(0, bodyWidth - sidebarThickness),
                    bodyHeight);
                break;

            case SidebarPosition.Right:
                context.FillRectangle(
                    SidebarBrush ?? Brushes.Transparent,
                    new Rect(1 + bodyWidth - sidebarThickness, bodyTop, sidebarThickness, bodyHeight));
                contentRect = new Rect(
                    1,
                    bodyTop,
                    Math.Max(0, bodyWidth - sidebarThickness),
                    bodyHeight);
                break;

            case SidebarPosition.Top:
                context.FillRectangle(
                    SidebarBrush ?? Brushes.Transparent,
                    new Rect(1, bodyTop, bodyWidth, sidebarThickness * 0.7));
                contentRect = new Rect(
                    1,
                    bodyTop + sidebarThickness * 0.7,
                    bodyWidth,
                    Math.Max(0, bodyHeight - sidebarThickness * 0.7));
                break;
        }

        context.FillRectangle(ContentBrush ?? Brushes.Transparent, contentRect);
        DrawContentLines(context, contentRect);
    }

    private void DrawContentLines(DrawingContext context, Rect contentRect)
    {
        if (AccentBrush is null || contentRect.Width <= 0 || contentRect.Height <= 0)
            return;

        var lineHeight = Math.Max(1.5, Bounds.Height * 0.035);
        var lineStep = lineHeight * 2;
        var startX = contentRect.X + contentRect.Width * 0.12;
        var startY = contentRect.Y + contentRect.Height * 0.18;

        for (var index = 0; index < 3; index++)
        {
            var widthFactor = index == 1 ? 0.5 : 0.8;
            var y = startY + index * lineStep;
            if (y + lineHeight > contentRect.Bottom)
                break;

            context.FillRectangle(
                AccentBrush,
                new Rect(startX, y, contentRect.Width * 0.76 * widthFactor, lineHeight));
        }
    }
}
