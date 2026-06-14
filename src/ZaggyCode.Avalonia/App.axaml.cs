using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using ZaggyCode.Avalonia.ViewModels;
using ZaggyCode.Avalonia.Views;

namespace ZaggyCode.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            if (this.ActualThemeVariant == ThemeVariant.Dark)
            {
                desktop.MainWindow.Background = this.Resources["DarkBackground"] as SolidColorBrush;
                desktop.MainWindow.ApplyTemplate();
                desktop.MainWindow.UpdateLayout();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}