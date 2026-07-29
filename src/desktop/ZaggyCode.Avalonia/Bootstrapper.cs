namespace ZaggyCode.Avalonia;

public sealed class Bootstrapper
{
    public async Task<IHost> LoadApplicationAsync()
    {
        var builder = Host.CreateApplicationBuilder(args: Environment.GetCommandLineArgs());

        builder.Configuration.AddJsonFile("appsettings.json");

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

        Assembly[] assemblies =
        [
            typeof(IUserStorage).Assembly,
            typeof(UserStorage).Assembly,
            typeof(Bootstrapper).Assembly
        ];

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
            .AddOptions<SpeedMillisecondsOptions>();

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
}
