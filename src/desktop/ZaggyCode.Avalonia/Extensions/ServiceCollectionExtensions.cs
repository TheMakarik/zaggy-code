using System.ComponentModel;
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
                TimeSpan.FromSeconds(storageOptions.Value.WaitUserDataUpdateSeconds),
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

    public static IServiceCollection AddUserDataStorage(this IServiceCollection services)
    {
        return services.AddObservableStorage<UserData>(
            options => options.DataFilePath,
            provider => provider.GetRequiredService<IOptions<DefaultUser>>().Value.User,
            Modules.Data.Json.UserDataSerializerContext.Default.UserData);
    }

    public static IServiceCollection AddPythonSettingsStorage(this IServiceCollection services)
    {
        return services.AddObservableStorage<PythonSettings>(
            options => options.PythonSettingsPath,
            provider => provider.GetRequiredService<IOptions<PythonDefaultSettingsOptions>>().Value.Settings,
            Modules.Data.Json.PythonSettingsSerializerContext.Default.PythonSettings);
    }
}