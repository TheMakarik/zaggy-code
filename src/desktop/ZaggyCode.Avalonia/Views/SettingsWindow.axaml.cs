using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace ZaggyCode.Avalonia.Views;

public partial class SettingsWindow : ReactiveWindow<SettingsViewModel>
{
    private bool _closeConfirmed;
    public SettingsWindow()
    {
        InitializeComponent();

        HeaderBar.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        MinimizeButton.Click += (_, __) => WindowState = WindowState.Minimized;
        MaximizeButton.Click += (_, __) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        CloseButton.Click += (_, __) =>
        {
            if (ViewModel is { HasChanges: true })
                _ = HandleCloseWithUnsavedChangesAsync();
            else
                Close();
        };

        PropertyChanged += (_, args) =>
        {
            if (args.Property.Name == nameof(WindowState))
            {
                MaximizeIcon.Kind = WindowState == WindowState.Maximized
                    ? Material.Icons.MaterialIconKind.WindowRestore
                    : Material.Icons.MaterialIconKind.WindowMaximize;
            }
        };

        this.DataContextChanged += (_, _) =>
        {
            if (ViewModel is null)
                return;

            ViewModel.CloseSettingsInteraction.RegisterHandler(context =>
            {
                Close();
                context.SetOutput(Unit.Default);
            });
        };
    }

    protected override void OnClosing(WindowClosingEventArgs eventArgs)
    {
        base.OnClosing(eventArgs);

        if (_closeConfirmed || eventArgs.IsProgrammatic || ViewModel is null || !ViewModel.HasChanges)
            return;

        eventArgs.Cancel = true;

        _ = HandleCloseWithUnsavedChangesAsync();
    }

    private async Task HandleCloseWithUnsavedChangesAsync()
    {
        var confirmationWindow = new ConfirmSaveSettingsChangesWindow
        {
            WindowDecorations = WindowDecorations,
            DataContext = new ConfirmSaveSettingsChangesViewModel()
        };
        var result = await confirmationWindow.ShowDialog<bool?>(this);

        if (result == true && ViewModel is not null)
            ViewModel.SaveSettingsCommand.Execute(new RxVoid()).Subscribe();
        else
            ViewModel?.RevertAppearanceChanges();

        _closeConfirmed = true;
        Close();
    }

    private void ResizeHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not string edgeText)
            return;

        if (!Enum.TryParse<WindowEdge>(edgeText, out var edge))
            return;

        BeginResizeDrag(edge, e);
    }
}
