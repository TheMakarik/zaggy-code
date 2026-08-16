namespace ZaggyCode.Avalonia.Views;

public partial class SettingsWindow : ReactiveWindow<SettingsViewModel>
{
    private TextMate.Installation? _csharpTextMateInstallation;
    private TextMate.Installation? _pythonTextMateInstallation;

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

            ViewModel.ApplyCodeThemeInteraction.RegisterHandler(context =>
            {
                ApplyCodeTheme(context.Input);
                context.SetOutput(context.Input);
            });

            InitializeExampleEditors();
        };
    }

    protected override void OnClosing(WindowClosingEventArgs eventArgs)
    {
        base.OnClosing(eventArgs);

        if (eventArgs.IsProgrammatic || ViewModel is null || !ViewModel.HasChanges)
            return;

        eventArgs.Cancel = true;

        _ = HandleCloseWithUnsavedChangesAsync();
    }

    private async Task HandleCloseWithUnsavedChangesAsync()
    {
        var confirmationWindow = new ConfirmSaveChangesWindow
        {
            WindowDecorations = WindowDecorations
        };
        var result = await confirmationWindow.ShowDialog<bool?>(this);

        if (result != true)
        {
            Close();
            return;
        }

        if (ViewModel is null)
            return;

        ViewModel.SaveSettingsCommand.Execute(Unit.Default).Subscribe();
    }

    private void InitializeExampleEditors()
    {
        if (!Enum.TryParse<ThemeName>(ViewModel!.SelectedCodeTheme, out var themeName))
            themeName = ThemeName.VisualStudioDark;

        var registryOptions = new RegistryOptions(themeName);

        _csharpTextMateInstallation = CSharpExampleEditor.InstallTextMate(registryOptions);
        _csharpTextMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cs").Id));

        _pythonTextMateInstallation = PythonExampleEditor.InstallTextMate(registryOptions);
        _pythonTextMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".py").Id));

        LoadExampleCode(CSharpExampleEditor, ViewModel!.CSharpExamplePath);
        LoadExampleCode(PythonExampleEditor, ViewModel.PythonExamplePath);
    }

    private static void LoadExampleCode(TextEditor editor, string filePath)
    {
        if (!File.Exists(filePath))
            return;

        editor.Text = File.ReadAllText(filePath);
    }

    private void ApplyCodeTheme(string themeName)
    {
        if (!Enum.TryParse<ThemeName>(themeName, out var theme))
            return;

        var registryOptions = new RegistryOptions(theme);
        _csharpTextMateInstallation?.SetTheme(registryOptions.LoadTheme(theme));
        _pythonTextMateInstallation?.SetTheme(registryOptions.LoadTheme(theme));
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
