namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ConfirmSaveSettingsChangesWindow : Window
{
    public ConfirmSaveSettingsChangesWindow()
    {
        InitializeComponent();

        CustomTitleBar.IsVisible = WindowDecorations != WindowDecorations.Full;

        DataContextChanged += (_, _) =>
        {
            if (DataContext is not ConfirmSaveSettingsChangesViewModel viewModel)
                return;

            viewModel.CloseInteraction.RegisterHandler(context =>
            {
                Close(context.Input);
                context.SetOutput(Unit.Default);
            });
        };
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void LoadZaggyIcon(object? sender, RoutedEventArgs e)
    {
        if (sender is not SvgFromContent control)
            return;

        control.Path = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.EmotionQuestion;
    }
}
