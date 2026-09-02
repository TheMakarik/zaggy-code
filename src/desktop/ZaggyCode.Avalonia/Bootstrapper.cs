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
            typeof(UserData).Assembly,
            typeof(GameCodeStorage).Assembly,
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

            .AddClasses(c => c.WithAttribute<LanguageAttribute>())
            .AsImplementedInterfaces()
            .WithServiceKey(type => type.GetCustomAttribute<LanguageAttribute>()!.Language)
            .WithScopedLifetime()

            .AddClasses(c => c.Where(t =>
                !t.IsAssignableTo(typeof(IDisposable)) &&
                !t.IsAssignableTo(typeof(IAsyncDisposable)) &&
                !t.IsAssignableTo(typeof(ViewModelBase)) &&
                !t.IsDefined(typeof(LanguageAttribute), false)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        );

        builder.Services
            .AddPythonSettingsStorage()
            .AddCSharpSettingsStorage()
            .AddUserDataStorage();

        builder.Logging
            .ClearProviders()
            .AddSerilog(Log.Logger, dispose: true);

        builder
            .AddOptions<FontSizeOptions>()
            .AddOptions<CodeExamplePathOptions>()
            .AddOptions<CodeThemeDisplayNameOptions>()
            .AddOptions<CodeThemeIconOptions>()
            .AddOptions<PopupOptions>()
            .AddOptions<DefaultUser>()
            .AddOptions<StorageOptions>()
            .AddOptions<SpeedMillisecondsOptions>()
            .AddOptions<MetadataOptions>()
            .AddOptions<TempOptions>()
            .AddOptions<LoggingCompressOptions>()
            .AddOptions<PythonScriptsOptions>()
            .AddOptions<PythonDefaultSettingsOptions>()
            .AddOptions<CSharpDefaultSettingsOptions>()
            .AddOptions<MapAssetsOptions>()
            .AddOptions<ZaggyAssetsOptions>()
            .AddOptions<PythonValidationOptions>()
            .AddOptions<ThemeOptions>()
            .AddOptions<LoadingOptions>();

        
        builder.Services.AddSingleton<IArchiveCompressor, TarBZip2ArchiveCompressor>();

        // Прокси создаются до готовности контролов главного окна, поэтому инстансы
        // заполняются через Attach после инициализации MainWindow.
        builder.Services.AddSingleton<RobotGameEditorProxy>();
        builder.Services.AddSingleton<IRobotGameEditorProxy>(provider => provider.GetRequiredService<RobotGameEditorProxy>());
        builder.Services.AddSingleton<RobotGameTerminalProxy>();
        builder.Services.AddSingleton<IRobotGameTerminalProxy>(provider => provider.GetRequiredService<RobotGameTerminalProxy>());

        var app = builder.Build();

        _ = app.RunAsync();

        await using var scope = app.Services.CreateAsyncScope();

#if DEBUG
        try
        {
#endif
            var storageFacade = scope.ServiceProvider.GetRequiredService<IStorageFacade>();
            await storageFacade.LoadAllAsync();
#if DEBUG
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
#endif



        return app;
    }

    // Ключ регистрации должен совпадать с ключом резолва в GameEngine — там раннеры
    // запрашиваются по enum Language, а не по строке расширения.
    private static Language GetLanguageKey(string extension)
    {
        return Enum.GetValues<Language>().First(language => language.GetLanguageExtension() == extension);
    }

    private static string GetDataDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "zaggy", "data");

        if (OperatingSystem.IsMacOS())
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "zaggy", "data");

        string? dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrEmpty(dataHome))
            return Path.Join(dataHome, "zaggy");

        return Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "share",
            "zaggy");
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

        var dataDirectory = GetDataDirectory();
        Directory.CreateDirectory(dataDirectory);
        Environment.SetEnvironmentVariable("ZAGGY_DATA", dataDirectory);

        var stateDirectory = GetStateDirectory();
        Directory.CreateDirectory(stateDirectory);
        Environment.SetEnvironmentVariable("ZAGGY_STATE", stateDirectory);
    }

    private static string GetTempDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Join(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(), "zaggy");

        if (OperatingSystem.IsMacOS())
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Caches", "zaggy");

        string? runtimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        if (!string.IsNullOrEmpty(runtimeDir))
            return Path.Join(runtimeDir, "zaggy");

        return Path.Join(Path.GetTempPath(), $"zaggy-{Environment.UserName}");
    }

    private static string GetConfigDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "zaggy");

        if (OperatingSystem.IsMacOS())
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "zaggy");

        string? configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrEmpty(configHome))
            return Path.Join(configHome, "zaggy");

        return Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "zaggy");
    }

    private static string GetStateDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "zaggy", "state");

        if (OperatingSystem.IsMacOS())
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Logs", "zaggy");

        string? stateHome = Environment.GetEnvironmentVariable("XDG_STATE_HOME");
        if (!string.IsNullOrEmpty(stateHome))
            return Path.Join(stateHome, "zaggy");

        return Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "state",
            "zaggy");
    }
}
