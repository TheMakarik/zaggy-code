using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ZaggyCode.Data.Interfaces;
using ZaggyCode.Data.Options;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Shared.Attributes;

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
        
        builder.Services.Scan(selector => selector
            .FromAssemblies(assemblies)
            .AddClasses(c => c.WithAttribute<SingletonServiceAttribute>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            
            .AddClasses(c => c.WithAttribute<SingletonServiceAttribute>())
            .AsSelf()
            .WithSingletonLifetime()
    
            .AddClasses(c => c.WithAttribute<ScopedServiceAttribute>()
                .WithoutAttribute<LanguageExtensionAttribute>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
    
            .AddClasses(c => c.WithAttribute<ScopedServiceAttribute>()
                .WithAttribute<LanguageExtensionAttribute>())
            .AsImplementedInterfaces()
            .WithServiceKey(t => t.GetCustomAttribute<LanguageExtensionAttribute>()!.Extension)
            .WithScopedLifetime()
    
            .AddClasses(c => c.WithAttribute<TransientServiceAttribute>())
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );
        
        builder.Logging
            .ClearProviders()
            .AddSerilog(Log.Logger, dispose: true);

        builder.Services
            .Configure<DefaultUser>(builder.Configuration.GetSection(nameof(DefaultUser)))
            .Configure<StorageOptions>(builder.Configuration.GetSection(nameof(StorageOptions)));
        
        var app = builder.Build();

        _ = app.RunAsync();
        
        await using var scope = app.Services.CreateAsyncScope();

        var storageFacade = scope.ServiceProvider.GetRequiredService<IStorageFacade>();
        await storageFacade.LoadAllAsync();

        return app;
    }
}