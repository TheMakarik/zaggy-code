using System.ComponentModel;
using ZaggyCode.Languages.Attributes;

namespace ZaggyCode.Languages.Enums;

public enum Language
{
    [LanguageExtension(".cs")]
    [LanguagePrettyName("C#")]
    CSharp,

    [LanguageExtension(".lua")]
    Lua,
    
    [LanguageExtension(".ss")]
    ShardScript
}

