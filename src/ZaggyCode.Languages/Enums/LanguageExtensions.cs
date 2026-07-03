using ZaggyCode.Languages.Attributes;

namespace ZaggyCode.Languages.Enums;

public static class LanguageExtensions
{
    public static string GetLanguageExtension(this Language language)
    {
        var field = typeof(Language).GetField(language.ToString());
        var attribute = field?.GetCustomAttributes(typeof(LanguageExtensionAttribute), false)
            .Cast<LanguageExtensionAttribute>()
            .FirstOrDefault();

        return attribute?.Extension ?? ".lua";
    }
}
