using ZaggyCode.Languages.Attributes;

namespace ZaggyCode.Languages.Enums;

public enum Language
{
    [LanguageExtension(".cs")]
    CSharp,

    [LanguageExtension(".lua")]
    Lua,

    [LanguageExtension(".ss")]
    ShardScript
}