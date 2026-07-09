namespace ZaggyCode.Core.Languages.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class LanguagePrettyNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}