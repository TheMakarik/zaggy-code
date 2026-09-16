namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ConfirmThemeSaveWindow : Window
{
    public ConfirmThemeSaveWindow()
    {
        InitializeComponent();
        this.ApplyThemeDecorations();

        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        CustomTitleBar.CloseRequested += (_, _) => Close(null);

        DataContextChanged += (_, _) =>
        {
            if (DataContext is not ConfirmThemeSaveViewModel viewModel)
                return;

            viewModel.CloseInteraction.RegisterHandler(context =>
            {
                Close(context.Input);
                context.SetOutput(Unit.Default);
            });
        };
    }
}
