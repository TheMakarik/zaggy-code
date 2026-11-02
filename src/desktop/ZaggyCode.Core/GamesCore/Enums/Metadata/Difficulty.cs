namespace ZaggyCode.Core.GamesCore.Enums.Metadata;

public enum Difficulty
{
    [XmlEnum("none")] None,
    [XmlEnum("very-easy")] VeryEasy,
    [XmlEnum("easy")] Easy,
    [XmlEnum("normal")] Normal,
    [XmlEnum("hard")] Hard,
    [XmlEnum("insane")] Insane
}
