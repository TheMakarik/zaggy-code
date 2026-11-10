namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        this.ApplyZaggyIcon();

        var viewModel = App.Services.GetRequiredService<AboutViewModel>();
        DataContext = viewModel;

        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        this.EnableWindowDrag();

        viewModel.OpenUrlInteraction.RegisterHandler(async context =>
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher is not null && Uri.TryCreate(context.Input, UriKind.Absolute, out var uri))
                await launcher.LaunchUriAsync(uri);

            context.SetOutput(Unit.Default);
        });
    }
}
