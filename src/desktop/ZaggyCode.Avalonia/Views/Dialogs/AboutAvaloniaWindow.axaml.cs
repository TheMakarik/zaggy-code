namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class AboutAvaloniaWindow : Window
{
    public AboutAvaloniaWindow()
    {
        InitializeComponent();
        this.ApplyThemeDecorations();

        var viewModel = App.Services.GetRequiredService<AboutAvaloniaViewModel>();
        DataContext = viewModel;

        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        viewModel.OpenUrlInteraction.RegisterHandler(async context =>
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher is not null && Uri.TryCreate(context.Input, UriKind.Absolute, out var uri))
                await launcher.LaunchUriAsync(uri);

            context.SetOutput(Unit.Default);
        });
    }
}
