namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ConfirmSaveSettingsChangesWindow : Window
{
    public ConfirmSaveSettingsChangesWindow()
    {
        InitializeComponent();
        CustomTitleBar.IsVisible = WindowDecorations != WindowDecorations.Full;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void LoadIcon(object? sender, RoutedEventArgs e)
    {
        var control = sender as SvgFromContent;
        control!.Path = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.EmotionQuestion;
    }
}
