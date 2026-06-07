using System.Xml.Serialization;

namespace ZaggyCode.Games.Enums;

public enum Difficulty
{
    [XmlEnum] VeryEasy,
    [XmlEnum] Easy,
    [XmlEnum] Normal,
    [XmlEnum] Hard,
    [XmlEnum] Insane
}