namespace ZaggyCode.Modules.Game;

//#:NO_AI
public sealed class RobotExecutor : IRobotExecutor
{
    private readonly Map _map;
    private Point _robotPoint;

    public RobotExecutor(Map map)
    {
        _map = map;
        _robotPoint = map.Points.First(point => point.IsSpawn);
        
        Debug.Assert(_robotPoint is { X: >= 1, Y: >= 1 });
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
        if (_robotPoint.RequireDraw)
        {
            DrawPoint?.Invoke(this, new DrawPointEventArgs { PointToDraw = _robotPoint });
            return;
        }

        RobotDied?.Invoke(this, new RobotDeadEventArgs { DiesType = RobotDiesType.DrawUnrequiredCell });
    }

    public bool IsCellFilled()
    {
        return _robotPoint.RequireDraw;
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
        var targetX = _robotPoint.X + deltaX;
        var targetY = _robotPoint.Y + deltaY;

        if (IsBeyondMap(targetX, targetY))
        {
            RaiseRobotDied(RobotDiesType.EndOfTheMap);
            return;
        }

        var target = FindPoint(targetX, targetY);
        if (target is null)
        {
            RaiseRobotDied(RobotDiesType.EndOfTheMap);
            return;
        }

        Debug.Assert(target.X >= 1 && target.Y >= 1);

        if (exitWall(_robotPoint.WallType) || enterWall(target.WallType) || target.WallType == WallType.Full)
        {
            RaiseRobotDied(RobotDiesType.Wall);
            return;
        }

        _robotPoint = target;
        RobotPointUpdated?.Invoke(this, new RobotPointUpdatedEventArgs { NewX = target.X, NewY = target.Y });
    }

    private bool HasWallInDirection(int deltaX, int deltaY, Func<WallType, bool> exitWall, Func<WallType, bool> enterWall)
    {
        var targetX = _robotPoint.X + deltaX;
        var targetY = _robotPoint.Y + deltaY;

        if (IsBeyondMap(targetX, targetY))
            return true;

        var target = FindPoint(targetX, targetY);
        if (target is null)
            return true;

        return exitWall(_robotPoint.WallType) || enterWall(target.WallType) || target.WallType == WallType.Full;
    }

    private void RaiseRobotDied(RobotDiesType diesType)
    {
        RobotDied?.Invoke(this, new RobotDeadEventArgs { DiesType = diesType });
    }

    private bool IsBeyondMap(int x, int y)
    {
        return x < 1 || y < 1 || x > _map.Width || y > _map.Height;
    }

    private Point? FindPoint(int x, int y)
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
