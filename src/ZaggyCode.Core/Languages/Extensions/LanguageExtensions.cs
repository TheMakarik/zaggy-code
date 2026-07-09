namespace ZaggyCode.Core.Languages.Extensions;

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