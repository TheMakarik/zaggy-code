namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ColorPickerWindow : Window
{
    public ColorPickerWindow()
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

        CustomTitleBar.CloseRequested += (_, _) => Close();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is not ColorPickerViewModel viewModel)
                return;

            viewModel.CloseInteraction.RegisterHandler(context =>
            {
                Close();
                context.SetOutput(Unit.Default);
            });
        };
    }
}
