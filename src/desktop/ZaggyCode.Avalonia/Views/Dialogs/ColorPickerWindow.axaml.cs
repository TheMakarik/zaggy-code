namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ColorPickerWindow : Window
{
    public ColorPickerWindow()
    {
        InitializeComponent();
        this.ApplyThemeDecorations();
        this.ApplyZaggyIcon();

        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        this.EnableWindowDrag();

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
