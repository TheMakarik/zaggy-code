namespace ZaggyCode.Avalonia;

public sealed class Bootstrapper
{
    public async Task<IHost> LoadApplicationAsync()
    {
        SetEnvironmentVariables();

        var builder = Host.CreateApplicationBuilder(args: Environment.GetCommandLineArgs());

        builder.Configuration.AddJsonFile("appsettings.json");

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

        Assembly[] assemblies =
        [
            typeof(IUserStorage).Assembly,
            typeof(UserStorage).Assembly,
            typeof(Bootstrapper).Assembly
        ];

        builder.Services.AddSingleton(typeof(XmlSerializer<>), typeof(XmlSerializer<>));
        builder.Services.AddHostedService<LoggingCompressHostedService>();

        builder.Services.Scan(selector => selector
            .FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo<ViewModelBase>())
            .AsSelf()
            .WithSingletonLifetime()

            .AddClasses(c => c.AssignableTo<IDisposable>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()

            .AddClasses(c => c.AssignableTo<IAsyncDisposable>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()

            .AddClasses(c => c.WithAttribute<LanguageExtensionAttribute>())
            .AsImplementedInterfaces()
            .WithServiceKey(type => type.GetCustomAttribute<LanguageExtensionAttribute>()!.Extension)
            .WithScopedLifetime()

            .AddClasses(c => c.Where(t =>
                !t.IsAssignableTo(typeof(IDisposable)) &&
                !t.IsAssignableTo(typeof(IAsyncDisposable)) &&
                !t.IsAssignableTo(typeof(ViewModelBase)) &&
                !t.IsDefined(typeof(LanguageExtensionAttribute), false)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        );

        builder.Logging
            .ClearProviders()
            .AddSerilog(Log.Logger, dispose: true);

        builder
            .AddOptions<FontSizeOptions>()
            .AddOptions<DefaultUser>()
            .AddOptions<StorageOptions>()
            .AddOptions<SpeedMillisecondsOptions>()
            .AddOptions<MetadataOptions>()
            .AddOptions<TempOptions>()
            .AddOptions<LoggingCompressOptions>();

        IHost app = builder.Build();

        _ = app.RunAsync();

        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

#if DEBUG
        try
        {
#endif
            IStorageFacade storageFacade = scope.ServiceProvider.GetRequiredService<IStorageFacade>();
            await storageFacade.LoadAllAsync();
#if DEBUG
        }
        catch (Exception e)
        {
            Log.Error(@$"
Произошла ошибка при загрузке пользовательских данных. Возможно это произошло из за требования к миграциям, которые пока что не реализованы
Самый простой способ удалить файл, который использовал сервис, и он будет создан по новой.
Например {Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), app.Services.GetRequiredService<IOptions<StorageOptions>>().Value.DataFilePath)} для {nameof(IUserStorage)}
");
            Console.WriteLine(e);
        }
#endif



        return app;
    }

    private static void SetEnvironmentVariables()
    {
        var appDirectory = AppContext.BaseDirectory;
        Environment.SetEnvironmentVariable("ZAGGY_APP", appDirectory);

        var tempDirectory = GetTempDirectory();
        Directory.CreateDirectory(tempDirectory);
        Environment.SetEnvironmentVariable("ZAGGY_TEMP", tempDirectory);

        var configDirectory = GetConfigDirectory();
        Directory.CreateDirectory(configDirectory);
        Environment.SetEnvironmentVariable("ZAGGY_CONFIG", configDirectory);

        var stateDirectory = GetStateDirectory();
        Directory.CreateDirectory(stateDirectory);
        Environment.SetEnvironmentVariable("ZAGGY_STATE", stateDirectory);
    }

    private static string GetTempDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(), "zaggy");

        if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Caches", "zaggy");

        string? runtimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        if (!string.IsNullOrEmpty(runtimeDir))
            return Path.Combine(runtimeDir, "zaggy");

        return Path.Combine(Path.GetTempPath(), $"zaggy-{Environment.UserName}");
    }

    private static string GetConfigDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "zaggy");

        if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "zaggy");

        string? configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrEmpty(configHome))
            return Path.Combine(configHome, "zaggy");

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "zaggy");
    }

    private static string GetStateDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "zaggy", "state");

        if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Logs", "zaggy");

        string? stateHome = Environment.GetEnvironmentVariable("XDG_STATE_HOME");
        if (!string.IsNullOrEmpty(stateHome))
            return Path.Combine(stateHome, "zaggy");

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "state",
            "zaggy");
    }
}
