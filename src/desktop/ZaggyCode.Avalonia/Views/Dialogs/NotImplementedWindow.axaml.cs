namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class NotImplementedWindow : Window
{
    public NotImplementedWindow()
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
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LoadZaggyIcon(object? sender, RoutedEventArgs e)
    {
        if (sender is not SvgFromContent control)
            return;

        control.Path = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.EmotionShock;
    }
}
