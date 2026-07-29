namespace ZaggyCode.Core.Common.Utils;

public sealed class XmlSerializer<T> : XmlSerializer
{
    private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));
    
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
        return (T?)_serializer.Deserialize(stream);
    }
    
    public new T? Deserialize(TextReader textReader)
    {
        return (T?)_serializer.Deserialize(textReader);
    }
    
    public new T? Deserialize(XmlReader xmlReader)
    {
        return (T?)_serializer.Deserialize(xmlReader);
    }
    
    public new T? Deserialize(XmlReader xmlReader, string encodingStyle)
    {
        return (T?)_serializer.Deserialize(xmlReader, encodingStyle);
    }
    
    public new T? Deserialize(XmlReader xmlReader, XmlDeserializationEvents events)
    {
        return (T?)_serializer.Deserialize(xmlReader, events);
    }
    
    public new T? Deserialize(XmlReader xmlReader, string encodingStyle, XmlDeserializationEvents events)
    {
        return (T?)_serializer.Deserialize(xmlReader, encodingStyle, events);
    }
    
    public new void Serialize(Stream stream, T? obj)
    {
        _serializer.Serialize(stream, obj);
    }
    
    public new void Serialize(TextWriter textWriter, T? obj)
    {
        _serializer.Serialize(textWriter, obj);
    }
    
    public new void Serialize(XmlWriter xmlWriter, T? obj)
    {
        _serializer.Serialize(xmlWriter, obj);
    }
    
    public new void Serialize(XmlWriter xmlWriter, T? obj, XmlSerializerNamespaces? namespaces)
    {
        _serializer.Serialize(xmlWriter, obj, namespaces);
    }
    
    public new void Serialize(XmlWriter xmlWriter, T? obj, XmlSerializerNamespaces? namespaces, string? encodingStyle)
    {
        _serializer.Serialize(xmlWriter, obj, namespaces, encodingStyle);
    }
    
    public new void Serialize(Stream stream, T? obj, XmlSerializerNamespaces? namespaces)
    {
        _serializer.Serialize(stream, obj, namespaces);
    }
    
    public new void Serialize(TextWriter textWriter, T? obj, XmlSerializerNamespaces? namespaces)
    {
        _serializer.Serialize(textWriter, obj, namespaces);
    }
}