namespace ZaggyCode.Modules.Game;

public sealed class RobotExecutor : IRobotExecutor
{
    private readonly Map _map;
    private RobotGamePoint _robotRobotGamePoint;

    public RobotExecutor(Map map)
    {
        _map = map;
        _robotRobotGamePoint = map.Points.First(point => point.IsSpawn);
        
        Debug.Assert(_robotRobotGamePoint is { X: >= 0, Y: >= 0 });
    }

    public EventHandler<RobotPointUpdatedEventArgs>? RobotPointUpdated { get; set; }
    public EventHandler<RobotDeadEventArgs>? RobotDied { get; set; }
    public EventHandler<DrawPointEventArgs>? DrawPoint { get; set; }

    public void MoveUp() => Move(0, -1);

    public void MoveRight() => Move(1, 0);

    public void MoveDown() => Move(0, 1);

    public void MoveLeft() => Move(-1, 0);

    public void FillCell()
    {
        if (_robotRobotGamePoint.RequireDraw)
        {
            DrawPoint?.Invoke(this, new DrawPointEventArgs { RobotGamePointToDraw = _robotRobotGamePoint });
            return;
        }

        RobotDied?.Invoke(this, new RobotDeadEventArgs { DeadType = RobotDeadType.DrawUnrequiredCell });
    }

    public bool IsCellFilled()
    {
        return _robotRobotGamePoint.RequireDraw;
    }

    public bool IsWallFromUp()
    {
        return HasWallInDirection(0, -1);
    }

    public bool IsWallFromDown()
    {
        return HasWallInDirection(0, 1);
    }

    public bool IsWallFromLeft()
    {
        return HasWallInDirection(-1, 0);
    }

    public bool IsWallFromRight()
    {
        return HasWallInDirection(1, 0);
    }
    
    private void Move(int deltaX, int deltaY)
    {
        var targetX = _robotRobotGamePoint.X + deltaX;
        var targetY = _robotRobotGamePoint.Y + deltaY;

        if (IsBeyondMap(targetX, targetY))
        {
            RaiseRobotDied(RobotDeadType.EndOfTheMap);
            return;
        }

        var target = FindPoint(targetX, targetY);
        if (target is null)
        {
            RaiseRobotDied(RobotDeadType.EndOfTheMap);
            return;
        }

        Debug.Assert(target.X >= 0 && target.Y >= 0);

        if (HasWallBetween(_robotRobotGamePoint, target))
        {
            RaiseRobotDied(RobotDeadType.Wall);
            return;
        }

        _robotRobotGamePoint = target;
        RobotPointUpdated?.Invoke(this, new RobotPointUpdatedEventArgs { NewX = target.X, NewY = target.Y });
    }

    private bool HasWallInDirection(int deltaX, int deltaY)
    {
        var targetX = _robotRobotGamePoint.X + deltaX;
        var targetY = _robotRobotGamePoint.Y + deltaY;

        if (IsBeyondMap(targetX, targetY))
            return true;

        var target = FindPoint(targetX, targetY);
        if (target is null)
            return true;

        return HasWallBetween(_robotRobotGamePoint, target);
    }

    private bool HasWallBetween(RobotGamePoint current, RobotGamePoint target)
    {
        if (target.WallType == WallType.Full)
            return true;

        var dx = target.X - current.X;
        var dy = target.Y - current.Y;

        if (dx == 1) return HasWallOnRight(current.WallType) || HasWallOnLeft(target.WallType);
        if (dx == -1) return HasWallOnLeft(current.WallType) || HasWallOnRight(target.WallType);
        if (dy == 1) return HasWallOnBottom(current.WallType) || HasWallOnTop(target.WallType);
        if (dy == -1) return HasWallOnTop(current.WallType) || HasWallOnBottom(target.WallType);

        return false;
    }

    private void RaiseRobotDied(RobotDeadType deadType)
    {
        RobotDied?.Invoke(this, new RobotDeadEventArgs { DeadType = deadType });
    }

    private bool IsBeyondMap(int x, int y)
    {
        return x < 0 || y < 0 || x >= _map.Width || y >= _map.Height;
    }

    private RobotGamePoint? FindPoint(int x, int y)
    {
        return _map.Points.FirstOrDefault(point => point.X == x && point.Y == y);
    }

    private static bool HasWallOnTop(WallType wallType)
    {
        return wallType is WallType.Full or WallType.Top or WallType.TopBottom or WallType.TopLeft
            or WallType.TopRight or WallType.TopBottomLeft or WallType.TopBottomRight or WallType.TopLeftRight;
    }

    private static bool HasWallOnBottom(WallType wallType)
    {
        return wallType is WallType.Full or WallType.Bottom or WallType.TopBottom or WallType.BottomLeft
            or WallType.BottomRight or WallType.TopBottomLeft or WallType.TopBottomRight or WallType.BottomLeftRight;
    }

    private static bool HasWallOnLeft(WallType wallType)
    {
        return wallType is WallType.Full or WallType.Left or WallType.TopLeft or WallType.BottomLeft
            or WallType.LeftRight or WallType.TopBottomLeft or WallType.TopLeftRight or WallType.BottomLeftRight;
    }

    private static bool HasWallOnRight(WallType wallType)
    {
        return wallType is WallType.Full or WallType.Right or WallType.TopRight or WallType.BottomRight
            or WallType.LeftRight or WallType.TopBottomRight or WallType.TopLeftRight or WallType.BottomLeftRight;
    }
}