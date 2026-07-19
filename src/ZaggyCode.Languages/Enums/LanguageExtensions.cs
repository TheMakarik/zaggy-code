using System.Reflection;
using ZaggyCode.Languages.Attributes;

namespace ZaggyCode.Languages.Enums;

public static class LanguageExtensions
{
    public static string GetLanguageExtension(this Language language)
    {
        FieldInfo? field = typeof(Language).GetField(language.ToString());
        LanguageExtensionAttribute? attribute = field?.GetCustomAttributes(typeof(LanguageExtensionAttribute), false)
            .Cast<LanguageExtensionAttribute>()
            .FirstOrDefault();

        return attribute?.Extension ?? ".lua";
    }
}
