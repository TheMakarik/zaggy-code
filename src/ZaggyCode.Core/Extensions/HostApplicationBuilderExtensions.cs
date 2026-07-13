namespace ZaggyCode.Core.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddOptions<T>(this IHostApplicationBuilder builder) where T : class
    {
       builder.Services.Configure<T>(builder.Configuration.GetSection(typeof(T).Name));
       return builder;
    }
}