using System.Drawing;
using ZaggyCode.Core.RobotGameCore.Enums;
using ZaggyCode.Core.RobotGameCore.Interfaces;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotExecutor : IRobotExecutor
{
    private readonly RobotMap _map;
    private readonly Dictionary<Point, RobotGamePoint> _points;

    public RobotExecutor(RobotMap map)
    {
        _map = map;
        _points = map.Points
            .GroupBy(point => new Point(point.X, point.Y))
            .ToDictionary(group => group.Key, group => group.First());

        var spawn = _points.Values.FirstOrDefault(point => point.IsSpawn)
            ?? throw new InvalidOperationException("На карте нет точки спавна робота.");

        State = new RobotGameMapState(new Point(spawn.X, spawn.Y));
    }

    public RobotGameMapState State { get; }

    public event EventHandler<RobotStateUpdate>? StateUpdated;

    public void Move(RobotDirection direction)
    {
        var (deltaX, deltaY) = GetDirectionDelta(direction);
        var currentPosition = State.Position;
        var targetPosition = new Point(currentPosition.X + deltaX, currentPosition.Y + deltaY);

        if (IsBeyondMap(targetPosition) || !_points.TryGetValue(targetPosition, out var target))
        {
            Raise(DeadAction(RobotDeadType.EndOfTheMap, direction));
            return;
        }

        if (HasWallBetween(currentPosition, targetPosition))
        {
            Raise(DeadAction(RobotDeadType.Wall, direction));
            return;
        }

        State.MoveTo(targetPosition);

        Point? collectedCoin = null;
        if (target.HasCoin && State.CollectCoin(targetPosition))
            collectedCoin = targetPosition;

        Raise(new RobotAction(
            Direction: direction,
            Position: targetPosition,
            CollectedCoin: collectedCoin,
            DrawnPoint: null,
            DeadType: null));
    }

    public void FillCell()
    {
        var position = State.Position;

        if (!_points.TryGetValue(position, out var point))
        {
            Raise(DeadAction(RobotDeadType.EndOfTheMap));
            return;
        }

        if (!point.RequireDraw)
        {
            Raise(DeadAction(RobotDeadType.DrawUnrequiredCell));
            return;
        }

        if (!State.MarkDrawn(position))
            return;

        Raise(new RobotAction(
            Direction: null,
            Position: position,
            CollectedCoin: null,
            DrawnPoint: position,
            DeadType: null));
    }

    public bool IsCellFilled() => State.IsDrawn(State.Position);

    public bool CheckWall(RobotDirection direction) => HasWallInDirection(direction);

    private bool HasWallInDirection(RobotDirection direction)
    {
        var (deltaX, deltaY) = GetDirectionDelta(direction);
        var currentPosition = State.Position;
        var targetPosition = new Point(currentPosition.X + deltaX, currentPosition.Y + deltaY);

        if (IsBeyondMap(targetPosition) || !_points.ContainsKey(targetPosition))
            return true;

        return HasWallBetween(currentPosition, targetPosition);
    }

    private bool HasWallBetween(Point currentPosition, Point targetPosition)
    {
        if (!_points.TryGetValue(currentPosition, out var current)
            || !_points.TryGetValue(targetPosition, out var target))
            return true;

        if (target.RobotWallType == RobotWallType.Full)
            return true;

        var deltaX = targetPosition.X - currentPosition.X;
        var deltaY = targetPosition.Y - currentPosition.Y;

        if (deltaX == 1)
            return HasWallOnRight(current.RobotWallType) || HasWallOnLeft(target.RobotWallType);

        if (deltaX == -1)
            return HasWallOnLeft(current.RobotWallType) || HasWallOnRight(target.RobotWallType);

        if (deltaY == 1)
            return HasWallOnBottom(current.RobotWallType) || HasWallOnTop(target.RobotWallType);

        if (deltaY == -1)
            return HasWallOnTop(current.RobotWallType) || HasWallOnBottom(target.RobotWallType);

        return false;
    }

    private RobotAction DeadAction(RobotDeadType deadType, RobotDirection? direction = null)
        => new(
            Direction: direction,
            Position: State.Position,
            CollectedCoin: null,
            DrawnPoint: null,
            DeadType: deadType);

    private void Raise(RobotAction action)
        => StateUpdated?.Invoke(this, RobotStateUpdate.From(action));

    private bool IsBeyondMap(Point position)
        => position.X < 0
            || position.Y < 0
            || position.X >= _map.Width
            || position.Y >= _map.Height;

    private static (int DeltaX, int DeltaY) GetDirectionDelta(RobotDirection direction)
        => direction switch
        {
            RobotDirection.Up => (0, -1),
            RobotDirection.Down => (0, 1),
            RobotDirection.Left => (-1, 0),
            RobotDirection.Right => (1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Неизвестное направление робота.")
        };

    private static bool HasWallOnTop(RobotWallType wallType)
        => wallType is RobotWallType.Full or RobotWallType.Top or RobotWallType.TopBottom
            or RobotWallType.TopLeft or RobotWallType.TopRight or RobotWallType.TopBottomLeft
            or RobotWallType.TopBottomRight or RobotWallType.TopLeftRight;

    private static bool HasWallOnBottom(RobotWallType wallType)
        => wallType is RobotWallType.Full or RobotWallType.Bottom or RobotWallType.TopBottom
            or RobotWallType.BottomLeft or RobotWallType.BottomRight or RobotWallType.TopBottomLeft
            or RobotWallType.TopBottomRight or RobotWallType.BottomLeftRight;

    private static bool HasWallOnLeft(RobotWallType wallType)
        => wallType is RobotWallType.Full or RobotWallType.Left or RobotWallType.TopLeft
            or RobotWallType.BottomLeft or RobotWallType.LeftRight or RobotWallType.TopBottomLeft
            or RobotWallType.TopLeftRight or RobotWallType.BottomLeftRight;

    private static bool HasWallOnRight(RobotWallType wallType)
        => wallType is RobotWallType.Full or RobotWallType.Right or RobotWallType.TopRight
            or RobotWallType.BottomRight or RobotWallType.LeftRight or RobotWallType.TopBottomRight
            or RobotWallType.TopLeftRight or RobotWallType.BottomLeftRight;
}
