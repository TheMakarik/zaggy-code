namespace ZaggyCode.Core.GamesRuntime.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class LanguageAttribute(Language language) : Attribute
{
    public Language Language { get; } = language;
}
