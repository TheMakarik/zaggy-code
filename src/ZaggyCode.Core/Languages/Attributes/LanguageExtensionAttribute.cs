namespace ZaggyCode.Core.Languages.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class LanguageExtensionAttribute(string extension) : Attribute
{
    public string Extension { get; } = extension;
}