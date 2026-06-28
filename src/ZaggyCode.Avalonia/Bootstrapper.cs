using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using ZaggyCode.Avalonia.Options;
using ZaggyCode.Data.Interfaces;
using ZaggyCode.Data.Options;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.Options;
using ZaggyCode.Shared.Attributes;
using ZaggyCode.Shared.Extensions;
using Scrutor;

namespace ZaggyCode.Avalonia;

public sealed class Bootstrapper
{
    public async Task<IHost> LoadApplicationAsync()
    {
        var builder = Host.CreateApplicationBuilder(args: Environment.GetCommandLineArgs());
        var assemblies = (IReadOnlyCollection<Assembly>)[
            typeof(Bootstrapper).Assembly,
            typeof(IStorage).Assembly,
            typeof(IGameEditor).Assembly,
            typeof(Language).Assembly];

        builder.Configuration.AddJsonFile("appsettings.json");
        
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
         
        /*
         * Мигрированме с ZaggyCode.Shared.Attributes требует удаление аттрибутов. Теперь сервисы сканируются
         * "по конвенция".
         * Ниже представлены эти конценции:
              * Классы заканчивающиеся на ViewMode добавлены как Singleton без интерфейса (обычная  вьбмодель)
              * IDisposable/IAsyncDisposable классы будут зарегестрированы как Scoped
              * Классы с аттрибутом LanguageExtension - Keyed Singleton со значением LanguageExtension.Extension в качестве ключа
              * В остальных слуаях - Singleton
         */
        
        builder.Services.Scan(selector => selector
            .FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo<IDisposable>())
            .AddClasses(c => c.AssignableTo<IAsyncDisposable>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            
            .AddClasses(c => c.Where(t => t.Name.EndsWith("ViewModel")))
            .AsSelf()
            .WithSingletonLifetime()
            
            .AddClasses(c => c.WithAttribute<LanguageExtensionAttribute>())
            .AsImplementedInterfaces()
            .WithServiceKey(type => type.GetCustomAttribute<LanguageExtensionAttribute>()!.Extension)
            .WithSingletonLifetime()
            
            .AddClasses(c => c.Where(t => 
                !t.IsAssignableTo(typeof(IDisposable)) && 
                    !t.IsAssignableTo(typeof(IAsyncDisposable))).WithoutAttribute<LanguageExtensionAttribute>())
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
        catch(Exception e)
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