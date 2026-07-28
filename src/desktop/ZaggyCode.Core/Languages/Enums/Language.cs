namespace ZaggyCode.Core.Languages.Enums;

public enum Language
{
    [LanguageExtension(".cs")]
    [LanguagePrettyName("C#")]
    CSharp,

    [LanguageExtension(".py")]
    [LanguagePrettyName("Python")]
    Python
}
