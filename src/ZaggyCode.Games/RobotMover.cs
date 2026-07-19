using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using ZaggyCode.Games.Events;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Games.Models;

namespace ZaggyCode.Games;

public sealed class RobotMover : IRobotMover
{
    private Point _zaggyCurrent;
    private FrozenDictionary<System.Drawing.Point, Point> _pointsCache;

    public RobotMover(ILogger<RobotMover> logger, Game game, RobotEvents events)
    {
        // _zaggyCurrent = game.Map.Points.First(p => p.IsSpawn);
        // logger.LogDebug("Caching points for game: {gamePath}", game.Path);
        // var points = new Dictionary<System.Drawing.Point, Point>(capacity: game.Map.Points.Count);
        // foreach (var mapPoint in game.Map.Points)
        //     points.Add((System.Drawing.Point)mapPoint, mapPoint);
        // _pointsCache = points.ToFrozenDictionary();

    }

    public void Left()
    {
       
    }
    
    public void Up()
    {
        throw new NotImplementedException();
    }

    public void Down()
    {
        throw new NotImplementedException();
    }

    public void Right()
    {
        throw new NotImplementedException();
    }
    
    private bool CanMoveLeft()
    {
        Point nextPoint = _pointsCache.GetValueRefOrNullRef(new System.Drawing.Point(_zaggyCurrent.X - 1, _zaggyCurrent.Y));
        return false;
    }

}