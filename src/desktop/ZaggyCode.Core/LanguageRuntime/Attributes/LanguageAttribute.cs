using ZaggyCode.Core.LanguageRuntime.Enums;

namespace ZaggyCode.Core.LanguageRuntime.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class LanguageAttribute(Language language) : Attribute
{
    public Language Language { get; } = language;
}
