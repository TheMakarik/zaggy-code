using System.Text.Json.Serialization.Metadata;

namespace ZaggyCode.Avalonia.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObservableStorage<TData, TOptions>(
        this IServiceCollection services,
        Func<TOptions, string> pathSelector,
        Func<IServiceProvider, TData> defaultValueFactory,
        JsonTypeInfo<TData> jsonTypeInfo)
        where TOptions : class 
        where TData : class, INotifyPropertyChanged
    {
        services.AddSingleton<IObservableStorage<TData>>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<TOptions>>();
            var folderProvider = provider.GetRequiredService<ISpecialFolderProvider>();
            var logger = provider.GetRequiredService<ILogger<ObservableStorage<TData>>>();
            var updateWaiter = provider.GetRequiredService<IUpdateStorageWaiter>();
            var storageOptions = provider.GetRequiredService<IOptions<StorageOptions>>();

            var fullPath = folderProvider.GetFolder(
                Environment.SpecialFolder.ApplicationData,
                pathSelector(options.Value));

            return new ObservableStorage<TData>(
                logger,
                fullPath,
                TimeSpan.FromSeconds(storageOptions.Value.WaitSettingsUpdateSeconds),
                jsonTypeInfo,
                updateWaiter,
                () => defaultValueFactory(provider));
        });

        return services;
    }

    public static IServiceCollection AddObservableStorage<TData>(
        this IServiceCollection services,
        Func<StorageOptions, string> pathSelector,
        Func<IServiceProvider, TData> defaultValueFactory,
        JsonTypeInfo<TData> jsonTypeInfo)
        where TData : class, INotifyPropertyChanged
    {
        return services.AddObservableStorage<TData, StorageOptions>(
            pathSelector, 
            defaultValueFactory, 
            jsonTypeInfo);
    }

    public static IServiceCollection AddStateSettingsStorage(this IServiceCollection services)
    {
        return services.AddObservableStorage<StateSettings>(
            options => options.StateSettingsPath,
            provider => provider.GetRequiredService<IOptions<DefaultStateSettings>>().Value.Settings,
            Modules.Data.Json.StateSettingsSerializerContext.Default.StateSettings);
    }

    public static IServiceCollection AddViewSettingsStorage(this IServiceCollection services)
    {
        return services.AddObservableStorage<ViewSettings>(
            options => options.ViewSettingsPath,
            provider => provider.GetRequiredService<IOptions<DefaultViewSettings>>().Value.Settings,
            Modules.Data.Json.ViewSettingsSerializerContext.Default.ViewSettings);
    }

    public static IServiceCollection AddPythonSettingsStorage(this IServiceCollection services)
    {
        return services.AddObservableStorage<PythonSettings>(
            options => options.PythonSettingsPath,
            provider => provider.GetRequiredService<IOptions<PythonDefaultSettingsOptions>>().Value.Settings,
            Modules.Data.Json.PythonSettingsSerializerContext.Default.PythonSettings);
    }

    public static IServiceCollection AddCSharpSettingsStorage(this IServiceCollection services)
    {
        return services.AddObservableStorage<CSharpSettings>(
            options => options.CSharpSettingsPath,
            provider => provider.GetRequiredService<IOptions<CSharpDefaultSettingsOptions>>().Value.Settings,
            Modules.Data.Json.CSharpSettingsSerializerContext.Default.CSharpSettings);
    }

    public static IServiceCollection AddThemeSettingsStorage(this IServiceCollection services)
    {
        return services.AddObservableStorage<ThemeSettings>(
            options => options.ThemeSettingsPath,
            provider => provider.GetRequiredService<IOptions<DefaultThemeSettings>>().Value.Settings,
            Modules.Data.Json.ThemeSettingsSerializerContext.Default.ThemeSettings);
    }
}