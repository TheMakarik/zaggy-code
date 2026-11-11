using Markdown.Avalonia.Full;

namespace ZaggyCode.Avalonia.Views.Behaviors;

public class MarkdownZoomBehavior : Behavior<MarkdownScrollViewer>
{
    public static readonly StyledProperty<double> DefaultFontSizeProperty =
        AvaloniaProperty.Register<MarkdownZoomBehavior, double>(nameof(DefaultFontSize), 14);

    public static readonly StyledProperty<double> MinFontSizeProperty =
        AvaloniaProperty.Register<MarkdownZoomBehavior, double>(nameof(MinFontSize), 6);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<MarkdownZoomBehavior, double>(nameof(FontSize), 14);

    public static readonly StyledProperty<double> MaxFontSizeProperty =
        AvaloniaProperty.Register<MarkdownZoomBehavior, double>(nameof(MaxFontSize), 30);

    public static readonly StyledProperty<double> ZoomStepProperty =
        AvaloniaProperty.Register<MarkdownZoomBehavior, double>(nameof(ZoomStep), 1);

    // Тема markdown для Fluent масштабирует все блоки от этого ресурса,
    // поэтому заголовки и обычный текст меняются согласованно.
    private const string FontSizeResourceKey = "ControlContentThemeFontSize";

    private ICommand? _updateFontSizeCommand;
    private IDisposable? _fontSizeSubscription;

    public static readonly DirectProperty<MarkdownZoomBehavior, ICommand?> UpdateFontSizeCommandProperty = AvaloniaProperty.RegisterDirect<MarkdownZoomBehavior, ICommand?>(
        nameof(UpdateFontSizeCommand), o => o.UpdateFontSizeCommand, (o, v) => o.UpdateFontSizeCommand = v);

    public ICommand? UpdateFontSizeCommand
    {
        get => _updateFontSizeCommand;
        set => SetAndRaise(UpdateFontSizeCommandProperty, ref _updateFontSizeCommand, value);
    }

    public double DefaultFontSize
    {
        get => GetValue(DefaultFontSizeProperty);
        set => SetValue(DefaultFontSizeProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public double MinFontSize
    {
        get => GetValue(MinFontSizeProperty);
        set => SetValue(MinFontSizeProperty, value);
    }

    public double MaxFontSize
    {
        get => GetValue(MaxFontSizeProperty);
        set => SetValue(MaxFontSizeProperty, value);
    }

    public double ZoomStep
    {
        get => GetValue(ZoomStepProperty);
        set => SetValue(ZoomStepProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is null)
            return;

        AssociatedObject.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);

        _fontSizeSubscription = this.GetObservable(FontSizeProperty)
            .Subscribe(ApplyFontSize);

        ApplyFontSize(FontSize);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is null)
            return;

        _fontSizeSubscription?.Dispose();
        _fontSizeSubscription = null;

        AssociatedObject.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        AssociatedObject.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        if ((e.KeyModifiers & KeyModifiers.Control) == 0)
            return;

        switch (e.Key)
        {
            case Key.OemPlus:
            case Key.Add:
                ChangeFontSize(ZoomStep);
                e.Handled = true;
                break;
            case Key.OemMinus:
            case Key.Subtract:
                ChangeFontSize(-ZoomStep);
                e.Handled = true;
                break;
            case Key.D0:
                ResetFontSize();
                e.Handled = true;
                break;
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (AssociatedObject is null)
            return;

        if ((e.KeyModifiers & KeyModifiers.Control) == 0)
            return;

        ChangeFontSize(e.Delta.Y > 0 ? ZoomStep : -ZoomStep);
        e.Handled = true;
    }

    private void ChangeFontSize(double delta)
    {
        if (AssociatedObject is null)
            return;

        var newSize = Math.Clamp(FontSize + delta, MinFontSize, MaxFontSize);
        ApplyFontSize(newSize);
        UpdateFontSizeCommand?.Execute(Convert.ToInt32(newSize));
    }

    private void ResetFontSize()
    {
        if (AssociatedObject is null)
            return;

        ApplyFontSize(DefaultFontSize);
        UpdateFontSizeCommand?.Execute(Convert.ToInt32(DefaultFontSize));
    }

    private void ApplyFontSize(double fontSize)
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.Resources[FontSizeResourceKey] = Math.Clamp(fontSize, MinFontSize, MaxFontSize);
    }
}
