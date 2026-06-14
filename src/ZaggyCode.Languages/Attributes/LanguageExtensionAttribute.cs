namespace ZaggyCode.Languages.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LanguageExtensionAttribute(string extension) : Attribute
{
    public string Extension { get; } = extension;
}