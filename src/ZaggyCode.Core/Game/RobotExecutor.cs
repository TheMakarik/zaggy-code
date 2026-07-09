namespace ZaggyCode.Core.Game;

public sealed class RobotExecutor : IRobotExecutor
{
    private Point _zaggyCurrent;
    private FrozenDictionary<System.Drawing.Point, Point> _pointsCache;

    public RobotExecutor(ILogger<RobotExecutor> logger, Models.Game game, RobotEvents events)
    {
        // _zaggyCurrent = game.Map.Points.First(p => p.IsSpawn);
        // logger.LogDebug("Caching points for game: {gamePath}", game.Path);
        // var points = new Dictionary<System.Drawing.Point, Point>(capacity: game.Map.Points.Count);
        // foreach (var mapPoint in game.Map.Points)
        //     points.Add((System.Drawing.Point)mapPoint, mapPoint);
        // _pointsCache = points.ToFrozenDictionary();

    }

    public void MoveUp()
    {
        throw new NotImplementedException();
    }

    public void MoveRight()
    {
        throw new NotImplementedException();
    }

    public void MoveDown()
    {
        throw new NotImplementedException();
    }

    public void MoveLeft()
    {
        throw new NotImplementedException();
    }

    public bool CanMoveUp()
    {
        throw new NotImplementedException();
    }

    public bool CanMoveRight()
    {
        throw new NotImplementedException();
    }

    public bool CanMoveDown()
    {
        throw new NotImplementedException();
    }

    public bool CanMoveLeft()
    {
        throw new NotImplementedException();
    }

    public void Draw()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromUp()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromDown()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromLeft()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromRight()
    {
        throw new NotImplementedException();
    }
}