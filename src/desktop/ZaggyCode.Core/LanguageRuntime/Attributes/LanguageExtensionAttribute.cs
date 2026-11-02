namespace ZaggyCode.Core.LanguageRuntime.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class LanguageExtensionAttribute(string extension) : Attribute
{
    public string Extension { get; init; } = extension;
}