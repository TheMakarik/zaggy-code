using System.Xml.Serialization;

namespace ZaggyCode.Games.Enums;

public enum Difficulty
{
    [XmlEnum("very-easy")] VeryEasy,
    [XmlEnum("easy")] Easy,
    [XmlEnum("normal")] Normal,
    [XmlEnum("hard")] Hard,
    [XmlEnum("insane")] Insane
}