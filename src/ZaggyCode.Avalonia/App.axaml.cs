using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
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
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

      
        desktop.MainWindow = new MainWindow();
        var loading = new Window();
        loading.Show();
        new Bootstrapper()
            .LoadApplicationAsync()
            .ContinueWith(async task =>
                await this.Dispatcher
                    .InvokeAsync(() =>
                    {
                        if (task.Exception is not null)
                            Console.WriteLine(
                                $"Error was happen while loading: {string.Join(", ", task.Exception.InnerExceptions)}");
                        loading.Close();

                        desktop.MainWindow.DataContext = task.Result
                            .Services
                            .GetRequiredService<MainWindowViewModel>();

                        base.OnFrameworkInitializationCompleted();
                    }));
    }
}