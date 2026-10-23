namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ThemeEditorWindow : Window
{
    private bool _closeConfirmed;

    public ThemeEditorWindow()
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
            if (DataContext is not ThemeEditorViewModel viewModel)
                return;

            viewModel.CloseInteraction.RegisterHandler(context =>
            {
                _closeConfirmed = true;
                Close();
                context.SetOutput(Unit.Default);
            });

            viewModel.OpenAboutInteraction.RegisterHandler(async context =>
            {
                await new AboutWindow().ShowDialog(this);
                context.SetOutput(Unit.Default);
            });

            viewModel.OpenAboutAvaloniaInteraction.RegisterHandler(async context =>
            {
                await new AboutAvaloniaWindow().ShowDialog(this);
                context.SetOutput(Unit.Default);
            });

            viewModel.OpenColorPicker.RegisterHandler(async context =>
            {
                var window = new ColorPickerWindow { DataContext = context.Input };
                await window.ShowDialog(this);
                context.SetOutput(Unit.Default);
            });

            viewModel.ViewXmlInteraction.RegisterHandler(context => context.SetOutput(Unit.Default));
            viewModel.EditMetadataInteraction.RegisterHandler(context => context.SetOutput(Unit.Default));
            viewModel.HelpInteraction.RegisterHandler(context => context.SetOutput(Unit.Default));
        };
    }

    protected override void OnClosing(WindowClosingEventArgs eventArgs)
    {
        base.OnClosing(eventArgs);

        if (_closeConfirmed || eventArgs.IsProgrammatic || DataContext is not ThemeEditorViewModel viewModel || !viewModel.IsDirty)
            return;

        eventArgs.Cancel = true;

        _ = ConfirmCloseAsync(viewModel);
    }

    private async Task ConfirmCloseAsync(ThemeEditorViewModel viewModel)
    {
        var dialog = new ConfirmThemeSaveWindow
        {
            WindowDecorations = WindowDecorations,
            DataContext = new ConfirmThemeSaveViewModel(viewModel.ThemeName)
        };

        var result = await dialog.ShowDialog<bool?>(this);
        if (result is null)
            return;

        if (result == true)
            await viewModel.SaveAsync();
        else
            await viewModel.CancelAsync();
    }
}
