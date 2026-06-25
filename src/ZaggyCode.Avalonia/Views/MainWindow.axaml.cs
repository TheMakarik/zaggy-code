using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using ZaggyCode.Avalonia.Views.Controls;

namespace ZaggyCode.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (this.FindControl<TerminalControl>("Terminal") is not { } terminal)
            return;

        terminal.TerminalInput += (_, args) =>
        {
            terminal.WriteLine("Terminal: " + args);
        };
        
        terminal.WriteLine("\x1b[38;2;75;0;130mДобро пожаловать в Zaggy's Code!\x1b[0m");
        
        var textEditor = this.FindControl<TextEditor>("Editor");
        var  registryOptions = new RegistryOptions(ThemeName.VisualStudioLight);
        var textMateInstallation = textEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".lua").Id));
        
    }
}
