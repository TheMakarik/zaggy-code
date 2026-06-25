using System.Reflection;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;

namespace ZaggyCode.Languages.Extensions;

public static class LanguageExtensions
{
    public static string GetExtension(this Language language)
    {
        return language
            .GetType()
            .GetField(language.ToString())!
            .GetCustomAttribute<LanguageExtensionAttribute>()!.
            Extension!;
    }
}