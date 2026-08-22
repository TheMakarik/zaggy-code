namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class NotImplementedWindow : Window
{
    public NotImplementedWindow()
    {
        InitializeComponent();

        CustomTitleBar.IsVisible = WindowDecorations != WindowDecorations.Full;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void LoadZaggyIcon(object? sender, RoutedEventArgs e)
    {
        if (sender is not SvgFromContent control)
            return;

        control.Path = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.EmotionShock;
    }
}
