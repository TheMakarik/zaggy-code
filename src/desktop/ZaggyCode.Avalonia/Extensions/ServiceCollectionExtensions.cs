using System.Text.Json.Serialization.Metadata;

namespace ZaggyCode.Avalonia.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsStorage<TData, TOptions>(
        this IServiceCollection services,
        Func<TOptions, string> pathSelector,
        Func<IServiceProvider, TData> defaultValueFactory,
        JsonTypeInfo<TData> jsonTypeInfo,
        BackgroundTaskKind backgroundKind)
        where TOptions : class
        where TData : class, INotifyPropertyChanged
    {
        services.AddSingleton<ISettingsStorage<TData>>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<TOptions>>();
            var folderProvider = provider.GetRequiredService<ISpecialFolderProvider>();
            var logger = provider.GetRequiredService<ILogger<SettingsStorage<TData>>>();
            var backgroundTaskHelper = provider.GetRequiredService<IBackgroundTaskHelper>();
            var storageOptions = provider.GetRequiredService<IOptions<StorageOptions>>();

            var fullPath = folderProvider.GetFolder(
                Environment.SpecialFolder.ApplicationData,
                pathSelector(options.Value));

            return new SettingsStorage<TData>(
                logger,
                fullPath,
                backgroundTaskHelper,
                backgroundKind,
                TimeSpan.FromSeconds(storageOptions.Value.WaitSettingsUpdateSeconds),
                jsonTypeInfo,
                () => defaultValueFactory(provider));
        });

        return services;
    }

    public static IServiceCollection AddSettingsStorage<TData>(
        this IServiceCollection services,
        Func<StorageOptions, string> pathSelector,
        Func<IServiceProvider, TData> defaultValueFactory,
        JsonTypeInfo<TData> jsonTypeInfo,
        BackgroundTaskKind backgroundKind)
        where TData : class, INotifyPropertyChanged
    {
        return services.AddSettingsStorage<TData, StorageOptions>(
            pathSelector,
            defaultValueFactory,
            jsonTypeInfo,
            backgroundKind);
    }

    public static IServiceCollection AddStateSettingsStorage(this IServiceCollection services)
    {
        return services.AddSettingsStorage<StateSettings>(
            options => options.StateSettingsPath,
            provider => provider.GetRequiredService<IOptions<DefaultStateSettings>>().Value.Settings,
            Modules.Data.Json.StateSettingsSerializerContext.Default.StateSettings,
            BackgroundTaskKind.FlushingStateSettings);
    }

    public static IServiceCollection AddViewSettingsStorage(this IServiceCollection services)
    {
        return services.AddSettingsStorage<ViewSettings>(
            options => options.ViewSettingsPath,
            provider => provider.GetRequiredService<IOptions<DefaultViewSettings>>().Value.Settings,
            Modules.Data.Json.ViewSettingsSerializerContext.Default.ViewSettings,
            BackgroundTaskKind.FlushingViewSettings);
    }

    public static IServiceCollection AddPythonSettingsStorage(this IServiceCollection services)
    {
        return services.AddSettingsStorage<PythonSettings>(
            options => options.PythonSettingsPath,
            provider => provider.GetRequiredService<IOptions<PythonDefaultSettingsOptions>>().Value.Settings,
            Modules.Data.Json.PythonSettingsSerializerContext.Default.PythonSettings,
            BackgroundTaskKind.FlushingPythonSettings);
    }

    public static IServiceCollection AddCSharpSettingsStorage(this IServiceCollection services)
    {
        return services.AddSettingsStorage<CSharpSettings>(
            options => options.CSharpSettingsPath,
            provider => provider.GetRequiredService<IOptions<CSharpDefaultSettingsOptions>>().Value.Settings,
            Modules.Data.Json.CSharpSettingsSerializerContext.Default.CSharpSettings,
            BackgroundTaskKind.FlushingCSharpSettings);
    }

    public static IServiceCollection AddThemeSettingsStorage(this IServiceCollection services)
    {
        return services.AddSettingsStorage<ThemeSettings>(
            options => options.ThemeSettingsPath,
            provider => provider.GetRequiredService<IOptions<DefaultThemeSettings>>().Value.Settings,
            Modules.Data.Json.ThemeSettingsSerializerContext.Default.ThemeSettings,
            BackgroundTaskKind.FlushingThemeSettings);
    }
}
