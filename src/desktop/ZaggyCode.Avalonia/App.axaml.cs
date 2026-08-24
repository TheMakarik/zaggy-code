namespace ZaggyCode.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services { get => field ?? throw new InvalidOperationException("Cannot get null service collection"); set; } 
        
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        var mainWindow = new MainWindow();
        desktop.MainWindow = mainWindow;

        var loading = new Window();
        loading.Show();
         
        base.OnFrameworkInitializationCompleted();

        try
        {
            var host = await new Bootstrapper().LoadApplicationAsync();

            mainWindow.Show();
            loading.Close();

            mainWindow.DataContext = host.Services.GetRequiredService<MainWindowViewModel>();
            Services = host.Services;
        }
        catch (Exception ex)
        {
            mainWindow.Show();
            loading.Close();
            System.Diagnostics.Debug.WriteLine($"Error while loading: {ex}");
            Log.Logger.Error(ex, "Error while loading application");
        }
    }
}