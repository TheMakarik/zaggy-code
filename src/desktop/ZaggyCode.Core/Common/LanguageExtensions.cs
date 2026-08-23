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

    public static string GetPrettyName(this Language language)
    {
        return language
            .GetType()
            .GetField(language.ToString())!
            .GetCustomAttribute<LanguagePrettyNameAttribute>()?.Name
            ?? language.ToString();
    }
}
