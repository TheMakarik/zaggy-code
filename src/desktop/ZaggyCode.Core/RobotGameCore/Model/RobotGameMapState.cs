using System.Collections.Immutable;

namespace ZaggyCode.Core.RobotGameCore.Model;

// Только данные карты: позиция робота, закрашенные точки и собранные монеты.
// Мутирует состояние исполнитель, UI читает его как источник истины.
public sealed class RobotGameMapState(Point spawnPosition)
{
    private readonly Lock _lock = new();
    private Point _position = spawnPosition;
    private ImmutableHashSet<Point> _drawnPoints = [];
    private ImmutableHashSet<Point> _collectedCoins = [];

    public Point Position
    {
        get
        {
            lock (_lock)
                return _position;
        }
    }

    public IReadOnlyCollection<Point> DrawnPoints
    {
        get
        {
            lock (_lock)
                return _drawnPoints;
        }
    }

    public IReadOnlyCollection<Point> CollectedCoins
    {
        get
        {
            lock (_lock)
                return _collectedCoins;
        }
    }

    public void Reset(Point spawn)
    {
        lock (_lock)
        {
            _position = spawn;
            _drawnPoints = [];
            _collectedCoins = [];
        }
    }

    public void MoveTo(Point position)
    {
        lock (_lock)
            _position = position;
    }

    public bool MarkDrawn(Point point)
    {
        lock (_lock)
        {
            if (_drawnPoints.Contains(point))
                return false;

            _drawnPoints = _drawnPoints.Add(point);
            return true;
        }
    }

    public bool UnmarkDrawn(Point point)
    {
        lock (_lock)
        {
            if (!_drawnPoints.Contains(point))
                return false;

            _drawnPoints = _drawnPoints.Remove(point);
            return true;
        }
    }

    public void ClearDrawn()
    {
        lock (_lock)
            _drawnPoints = [];
    }

    public bool CollectCoin(Point point)
    {
        lock (_lock)
        {
            if (_collectedCoins.Contains(point))
                return false;

            _collectedCoins = _collectedCoins.Add(point);
            return true;
        }
    }

    public bool UncollectCoin(Point point)
    {
        lock (_lock)
        {
            if (!_collectedCoins.Contains(point))
                return false;

            _collectedCoins = _collectedCoins.Remove(point);
            return true;
        }
    }

    public void ClearCollected()
    {
        lock (_lock)
            _collectedCoins = [];
    }

    public bool IsDrawn(Point point)
    {
        lock (_lock)
            return _drawnPoints.Contains(point);
    }

    public bool IsCoinCollected(Point point)
    {
        lock (_lock)
            return _collectedCoins.Contains(point);
    }
}
