using System.Xml.Serialization;
using ZaggyCode.Games.Enums;

namespace ZaggyCode.Games.Models;

[XmlRoot("game")]
public sealed class Game
{
    [XmlAttribute("name")]
    public required string Name { get; set; }
    
    [XmlIgnore]
    public string? Path { get; set; }
    
    [XmlAttribute("description")]
    public required string Description { get; set; }
    
    [XmlAttribute("author")]
    public required string Author { get; set; }
    
    [XmlElement("map")]
    public required Map Map { get; set; }
    
    [XmlAttribute("difficulty")]
    public required Difficulty Difficulty { get; set; }
}