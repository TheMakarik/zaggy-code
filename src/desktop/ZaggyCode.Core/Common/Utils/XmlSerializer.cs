namespace ZaggyCode.Core.Common.Utils;

public sealed class XmlSerializer<T> : XmlSerializer
{
    private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));
    
    public XmlSerializer() : base(typeof(T))
    {
    }
    
    public XmlSerializer(XmlRootAttribute? root) : base(typeof(T), root)
    {
    }
    
    public XmlSerializer(Type[]? extraTypes) : base(typeof(T), extraTypes)
    {
    }
    
    public XmlSerializer(XmlAttributeOverrides? overrides) : base(typeof(T), overrides)
    {
    }
    
    public new T? Deserialize(Stream stream)
    {
        return (T?)Serializer.Deserialize(stream);
    }
    
    public new T? Deserialize(TextReader textReader)
    {
        return (T?)Serializer.Deserialize(textReader);
    }
    
    public new T? Deserialize(XmlReader xmlReader)
    {
        return (T?)Serializer.Deserialize(xmlReader);
    }
    
    public new T? Deserialize(XmlReader xmlReader, string encodingStyle)
    {
        return (T?)Serializer.Deserialize(xmlReader, encodingStyle);
    }
    
    public new T? Deserialize(XmlReader xmlReader, XmlDeserializationEvents events)
    {
        return (T?)Serializer.Deserialize(xmlReader, events);
    }
    
    public new T? Deserialize(XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
    {
        return (T?)Serializer.Deserialize(xmlReader, encodingStyle, events);
    }
    
    public void Serialize(Stream stream, T? obj)
    {
        Serializer.Serialize(stream, obj);
    }
    
    public void Serialize(TextWriter textWriter, T? obj)
    {
        Serializer.Serialize(textWriter, obj);
    }
    
    public void Serialize(XmlWriter xmlWriter, T? obj)
    {
        Serializer.Serialize(xmlWriter, obj);
    }
    
    public void Serialize(XmlWriter xmlWriter, T? obj, XmlSerializerNamespaces? namespaces)
    {
        Serializer.Serialize(xmlWriter, obj, namespaces);
    }
    
    public void Serialize(XmlWriter xmlWriter, T? obj, XmlSerializerNamespaces? namespaces, string? encodingStyle)
    {
        Serializer.Serialize(xmlWriter, obj, namespaces, encodingStyle);
    }
    
    public void Serialize(Stream stream, T? obj, XmlSerializerNamespaces? namespaces)
    {
        Serializer.Serialize(stream, obj, namespaces);
    }
    
    public void Serialize(TextWriter textWriter, T? obj, XmlSerializerNamespaces? namespaces)
    {
        Serializer.Serialize(textWriter, obj, namespaces);
    }
}