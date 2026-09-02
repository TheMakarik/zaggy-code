namespace ZaggyCode.Core.Languages.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class LanguageAttribute(Language language) : Attribute
{
    public Language Language { get; } = language;
}
