using System.Xml.Serialization;

namespace ZaggyCode.Games.Models;

public sealed class Point
{
    [XmlAttribute("x")]
    public int X { get; set; }
    
    [XmlAttribute("y")]
    public int Y { get; set; }
    
    [XmlAttribute("wall")]
    public bool IsWall { get; set; }
    
    [XmlAttribute("want-draw")]
    public bool RequireDraw { get; set; }
    
    [XmlAttribute("spawn")]
    public bool IsSpawn { get; set; }
    
    [XmlAttribute("coin")]
    public bool HasCoin { get; set; }
}