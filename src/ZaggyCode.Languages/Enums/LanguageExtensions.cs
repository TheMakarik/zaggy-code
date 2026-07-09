using System.Reflection;
using ZaggyCode.Languages.Attributes;

namespace ZaggyCode.Languages.Enums;

public static class LanguageExtensions
{
    public static string GetLanguageExtension(this Language language)
    {
        var field = typeof(Language).GetField(language.ToString());
        return field?.GetCustomAttribute<LanguageExtensionAttribute>()!.Extension;
    }
}
