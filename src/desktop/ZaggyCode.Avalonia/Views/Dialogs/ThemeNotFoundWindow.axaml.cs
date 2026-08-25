namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ThemeNotFoundWindow : Window
{
    public ThemeNotFoundWindow()
        : this(string.Empty)
    {
    }

    public ThemeNotFoundWindow(string missingThemeName)
    {
        InitializeComponent();

        if (!string.IsNullOrEmpty(missingThemeName))
            MessageText.Text = $"Тема \"{missingThemeName}\" не была найдена. Была выбрана стандартная тема Primus.";

        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
        => Close();

    private void LoadZaggyIcon(object? sender, RoutedEventArgs e)
    {
        if (sender is not SvgFromContent control)
            return;

        control.Path = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.EmotionSad;
    }
}
