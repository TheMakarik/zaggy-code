namespace ZaggyCode.Core.Game.Enums;


public enum WallType
{
    [XmlEnum("none")]
    None,

    [XmlEnum("full")]
    Full,

    [XmlEnum("top")]
    Top,

    [XmlEnum("bottom")]
    Bottom,

    [XmlEnum("left")]
    Left,

    [XmlEnum("right")]
    Right,

    [XmlEnum("top&bottom")]
    TopBottom,

    [XmlEnum("top&left")]
    TopLeft,

    [XmlEnum("top&right")]
    TopRight,

    [XmlEnum("bottom&left")]
    BottomLeft,

    [XmlEnum("bottom&right")]
    BottomRight,

    [XmlEnum("left&right")]
    LeftRight,

    [XmlEnum("top&bottom&left")]
    TopBottomLeft,

    [XmlEnum("top&bottom&right")]
    TopBottomRight,

    [XmlEnum("top&left&right")]
    TopLeftRight,

    [XmlEnum("bottom&left&right")]
    BottomLeftRight
}
