namespace ZaggyCode.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services { get => field ?? throw new InvalidOperationException("Cannot get null service collection"); set; } 
        
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

                        //Не надо переносить это в констуктор MainWindow. MainWindow создается ДО регистрации всех сервисов. Если создать экземпляр MainWindow внутри вызовва
                        //диспатчера, то Show почему то работать не будет (Show вызывается в OnFrameworkInitializationCompleted())
                        desktop.MainWindow.DataContext = task.Result
                            .Services
                            .GetRequiredService<MainWindowViewModel>();

                        Services = task.Result.Services;

                        base.OnFrameworkInitializationCompleted();
                    }));
    }
}