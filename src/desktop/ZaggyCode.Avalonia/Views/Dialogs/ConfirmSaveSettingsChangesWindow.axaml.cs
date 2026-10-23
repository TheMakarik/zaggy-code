namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ConfirmSaveSettingsChangesWindow : Window
{
    public ConfirmSaveSettingsChangesWindow()
    {
        InitializeComponent();
        this.ApplyThemeDecorations();
        this.ApplyZaggyIcon();
        
        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        this.EnableWindowDrag();

        CustomTitleBar.CloseRequested += (_, _) => Close(false);

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

    private void LoadZaggyIcon(object? sender, RoutedEventArgs e)
    {
        if (sender is not SvgFromContent control)
            return;

        control.Path = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.EmotionQuestion;
    }
}
