namespace ZaggyCode.Core.Common;

public static class LanguageExtensions
{
    public static string GetLanguageExtension(this Language language)
    {
        var languageType = typeof(Language);
        var languageName = language.ToString();
        var languageField = languageType.GetField(languageName)!;
        var extensionAttribute = languageField.GetCustomAttribute<LanguageExtensionAttribute>()!;
        var fileExtension = extensionAttribute.Extension;
    
        return fileExtension;
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
