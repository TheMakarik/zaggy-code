using System.Xml.Serialization;

namespace ZaggyCode.Games.Models;

public sealed class Map
{
    [XmlArray("point")]
    public required ICollection<Point> Points { get => field ?? []; set ; }
    
    [XmlAttribute("width-points")]
    public int Width { get; set; }
    
    [XmlAttribute("height-points")]
    public int Height { get; set; }
}