namespace ZaggyCode.Modules.Game;

//#:NO_AI
public sealed class RobotExecutor : IRobotExecutor
{
    private readonly Map _map;
    private RobotGamePoint _robotRobotGamePoint;

    public RobotExecutor(Map map)
    {
        _map = map;
        _robotRobotGamePoint = map.Points.First(point => point.IsSpawn);
        
        Debug.Assert(_robotRobotGamePoint is { X: >= 1, Y: >= 1 });
    }

    public EventHandler<RobotPointUpdatedEventArgs>? RobotPointUpdated { get; set; }
    public EventHandler<RobotDeadEventArgs>? RobotDied { get; set; }
    public EventHandler<DrawPointEventArgs>? DrawPoint { get; set; }

    public void MoveUp() => Move(0, -1, HasWallOnTop, HasWallOnBottom);

    public void MoveRight() => Move(1, 0, HasWallOnRight, HasWallOnLeft);

    public void MoveDown() => Move(0, 1, HasWallOnBottom, HasWallOnTop);

    public void MoveLeft() => Move(-1, 0, HasWallOnLeft, HasWallOnRight);

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
        return HasWallInDirection(0, -1, HasWallOnTop, HasWallOnBottom);
    }

    public bool IsWallFromDown()
    {
        return HasWallInDirection(0, 1, HasWallOnBottom, HasWallOnTop);
    }

    public bool IsWallFromLeft()
    {
        return HasWallInDirection(-1, 0, HasWallOnLeft, HasWallOnRight);
    }

    public bool IsWallFromRight()
    {
        return HasWallInDirection(1, 0, HasWallOnRight, HasWallOnLeft);
    }
    
    // Границы карты задаются Width/Height, а не набором точек: точек за пределами может быть больше.
    private void Move(int deltaX, int deltaY, Func<WallType, bool> exitWall, Func<WallType, bool> enterWall)
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

        Debug.Assert(target.X >= 1 && target.Y >= 1);

        if (exitWall(_robotRobotGamePoint.WallType) || enterWall(target.WallType) || target.WallType == WallType.Full)
        {
            RaiseRobotDied(RobotDeadType.Wall);
            return;
        }

        _robotRobotGamePoint = target;
        RobotPointUpdated?.Invoke(this, new RobotPointUpdatedEventArgs { NewX = target.X, NewY = target.Y });
    }

    private bool HasWallInDirection(int deltaX, int deltaY, Func<WallType, bool> exitWall, Func<WallType, bool> enterWall)
    {
        var targetX = _robotRobotGamePoint.X + deltaX;
        var targetY = _robotRobotGamePoint.Y + deltaY;

        if (IsBeyondMap(targetX, targetY))
            return true;

        var target = FindPoint(targetX, targetY);
        if (target is null)
            return true;

        return exitWall(_robotRobotGamePoint.WallType) || enterWall(target.WallType) || target.WallType == WallType.Full;
    }

    private void RaiseRobotDied(RobotDeadType deadType)
    {
        RobotDied?.Invoke(this, new RobotDeadEventArgs { DeadType = deadType });
    }

    private bool IsBeyondMap(int x, int y)
    {
        return x < 1 || y < 1 || x > _map.Width || y > _map.Height;
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
