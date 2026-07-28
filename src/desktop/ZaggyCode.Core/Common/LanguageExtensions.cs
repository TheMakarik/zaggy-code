using ZaggyCode.Core.Languages.Attributes;
using ZaggyCode.Core.Languages.Enums;

namespace ZaggyCode.Core.Common;

public static class LanguageExtensions
{
    public static string GetLanguageExtension(this Language language)
    {
        return language
            .GetType()
            .GetField(language.ToString())!
            .GetCustomAttribute<LanguageExtensionAttribute>()!.
            Extension!;
    }
}
