using ZaggyCode.Games.Models;

namespace ZaggyCode.Avalonia.UiModel;

public sealed class ActualGamePoint : Point
{
    public bool IsZaggyHere { get; set; }

    public static ActualGamePoint FromPoint(Point point)
    {
        return new ActualGamePoint()
        {
            X = point.X,
            Y = point.Y,
            HasCoin = point.HasCoin,
            IsSpawn = point.IsSpawn,
            IsZaggyHere = point.IsSpawn,
            RequireDraw = point.RequireDraw,
            IsWall = point.IsWall,
        };
    }
}