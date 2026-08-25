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

            // Services должен быть готов до назначения DataContext:
            // MainWindow при этом сразу прокидывает контролы в игровые прокси.
            Services = host.Services;
            mainWindow.DataContext = host.Services.GetRequiredService<MainWindowViewModel>();

            _ = ApplyStartupThemeAsync(host.Services);
        }
        catch (Exception ex)
        {
            mainWindow.Show();
            loading.Close();
            System.Diagnostics.Debug.WriteLine($"Error while loading: {ex}");
            Log.Logger.Error(ex, "Error while loading application");
        }
    }

    private static async Task ApplyStartupThemeAsync(IServiceProvider services)
    {
        try
        {
            await services.GetRequiredService<IThemeApplier>().ApplySavedThemeAsync();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to apply startup theme");
        }
    }
}