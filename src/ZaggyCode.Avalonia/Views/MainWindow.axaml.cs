using Avalonia.Controls;
using Avalonia.Interactivity;
using ZaggyCode.Avalonia.Controls;

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
        {
            return;
        }

        /*
        terminal.TerminalInput += (_, input) =>
        {
            terminal.Write(input);
        };
        */

        terminal.WriteLine("\x1b[1;32mДобро пожаловать в виртуальный терминал ZaggyCode!\x1b[0m");
        terminal.WriteLine("");
        terminal.WriteLine("\x1b[31mКрасный\x1b[0m \x1b[33mжелтый\x1b[0m \x1b[34mсиний\x1b[0m \x1b[38;2;255;100;200mRGB розовый\x1b[0m");
        terminal.WriteLine("");
        terminal.Write("\x1b[1m>\x1b[0m ");
    }
}
