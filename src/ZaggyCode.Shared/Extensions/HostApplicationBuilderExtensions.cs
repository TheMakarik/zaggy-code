using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ZaggyCode.Shared.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddOptions<T>(this IHostApplicationBuilder builder) where T : class
    {
       builder.Services.Configure<T>(builder.Configuration.GetSection(typeof(T).Name));
       return builder;
    }
}