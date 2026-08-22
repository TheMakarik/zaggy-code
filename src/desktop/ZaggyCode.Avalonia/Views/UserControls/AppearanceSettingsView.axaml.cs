namespace ZaggyCode.Avalonia.Views.UserControls;

public partial class AppearanceSettingsView : ReactiveUserControl<AppearanceSettingsViewModel>
{
    private TextMate.Installation? _csharpTextMateInstallation;
    private TextMate.Installation? _pythonTextMateInstallation;

    public AppearanceSettingsView()
    {
        InitializeComponent();

        this.DataContextChanged += (_, _) =>
        {
            if (ViewModel is null)
                return;

            ViewModel.ApplyCodeThemeInteraction.RegisterHandler(context =>
            {
                ApplyCodeTheme(context.Input);
                context.SetOutput(context.Input);
            });

            InitializeExampleEditors();
        };
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

        LoadExampleCode(CSharpExampleEditor, ViewModel.CSharpExamplePath);
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
}
