namespace ZaggyCode.Core.LanguageRuntime.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public sealed class LanguagePrettyNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
