namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ConfirmSaveSettingsChangesWindow : Window
{
    public ConfirmSaveSettingsChangesWindow()
    {
        InitializeComponent();

        // WindowDecorations из object initializer применяется после конструктора,
        // поэтому следим за изменением, а не проверяем разово.
        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        // Перетаскивание за любую область окна: клики по кнопкам не доходят сюда,
        // потому что Button помечает PointerPressed как обработанный.
        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

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
