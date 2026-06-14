using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace ZaggyCode.Avalonia.Views.Behaviors;

public class TextEditorZoomBehavior : Behavior<TextEditor>
{
    public static readonly StyledProperty<double> DefaultFontSizeProperty =
        AvaloniaProperty.Register<TextEditorZoomBehavior, double>(nameof(DefaultFontSize), 12);

    public static readonly StyledProperty<double> MinFontSizeProperty =
        AvaloniaProperty.Register<TextEditorZoomBehavior, double>(nameof(MinFontSize), 6);

    public static readonly StyledProperty<double> MaxFontSizeProperty =
        AvaloniaProperty.Register<TextEditorZoomBehavior, double>(nameof(MaxFontSize), 30);

    public static readonly StyledProperty<double> ZoomStepProperty =
        AvaloniaProperty.Register<TextEditorZoomBehavior, double>(nameof(ZoomStep), 1);

    public double DefaultFontSize
    {
        get => GetValue(DefaultFontSizeProperty);
        set => SetValue(DefaultFontSizeProperty, value);
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
        if (AssociatedObject is null) return;
            
        AssociatedObject.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is null) return;
            
        AssociatedObject.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        AssociatedObject.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (AssociatedObject is null) return;

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

    private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        if (AssociatedObject is null) return;

        if ((e.KeyModifiers & KeyModifiers.Control) == 0)
            return;
        ChangeFontSize(e.Delta.Y > 0 ? ZoomStep : -ZoomStep);
        e.Handled = true;
    }

    private void ChangeFontSize(double delta)
    {
        if (AssociatedObject is null) return;
            
        var newSize = Math.Clamp(AssociatedObject.FontSize + delta, MinFontSize, MaxFontSize);
        AssociatedObject.FontSize = newSize;
        AssociatedObject.TextArea.FontSize = newSize;
    }

    private void ResetFontSize()
    {
        if (AssociatedObject is null) return;
            
        AssociatedObject.FontSize = DefaultFontSize;
        AssociatedObject.TextArea.FontSize = DefaultFontSize;
    }
}