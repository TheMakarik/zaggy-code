using System.Diagnostics;
using System.Reactive.Disposables.Fluent;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using ReactiveUI;
using ReactiveUI.Avalonia;
using TextMateSharp.Grammars;
using ZaggyCode.Avalonia.ViewModels;
using ZaggyCode.Avalonia.Views.Controls;

namespace ZaggyCode.Avalonia.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.DataContextChanged += (_, __) =>
        {
            ViewModel!.ClearTerminalContent.RegisterHandler(_ =>
            {
                Terminal.XTermDotNetTerminal.Clear();
            });
        };

    }
    

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        
        Terminal.WriteLine("\x1b[38;2;75;0;130mДобро пожаловать в Zaggy's Code!\x1b[0m");
        
        var  registryOptions = new RegistryOptions(ThemeName.VisualStudioLight);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".lua").Id));
        
    }
}
