using ZaggyCode.Core.LanguageRuntime.Attributes;

namespace ZaggyCode.Core.LanguageRuntime.Enums;

public enum Language
{
    [LanguagePrettyName("C#")]
    [LanguageExtension(".cs")]
    CSharp,
    
    [LanguageExtension(".py")]
    Python
}
