using System.Collections.Concurrent;

namespace ZaggyCode.Modules.Data;

//#:NO_AI
public static class SettingsCopier
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();

    public static void Copy<T>(T source, T target)
    {
        foreach (var property in Properties.GetOrAdd(typeof(T), static type => type.GetProperties()
                     .Where(property => property is { CanRead: true, CanWrite: true } &&
                                        property.GetIndexParameters().Length == 0)
                     .ToArray()))
            property.SetValue(target, property.GetValue(source));
    }
}
