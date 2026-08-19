namespace ZaggyCode.Avalonia.Views.Controls;

public enum RobotDirection
{
    Up,
    Down,
    Left,
    Right
}

public enum RobotState
{
    Idle,
    Moving,
    Dying,
    Dead
}

public sealed class CellPaintedEventArgs : EventArgs
{
    public int Column { get; init; }
    public int Row { get; init; }
}

public sealed class MapRobotStateMachine : IRobotExecutor
{
    private record struct MapPoint(int X, int Y);
    private record struct CellWalls(bool Top, bool Bottom, bool Left, bool Right);

    private readonly RobotEvents _events = new();
    private readonly Lock _stateLock = new();
    private readonly HashSet<MapPoint> _painted = [];
    private readonly HashSet<MapPoint> _collected = [];

    private Map? _map;
    private GamePoint?[,] _cells = new GamePoint?[0, 0];
    private int _cols;
    private int _rows;

    private int _logicalCol;
    private int _logicalRow;
    private int _spawnCol;
    private int _spawnRow;
    private RobotDirection _direction = RobotDirection.Down;
    private RobotState _state = RobotState.Idle;
    private bool _completed;

    public int ColumnCount => _cols;
    public int RowCount => _rows;
    public int LogicalColumn => _logicalCol;
    public int LogicalRow => _logicalRow;
    public RobotDirection Direction => _direction;
    public RobotState State => _state;
    public bool IsDead => _state == RobotState.Dead;
    public bool IsCompleted => _completed;
    public int DeathDirectionColumn { get; private set; }
    public int DeathDirectionRow { get; private set; }
    public RobotEvents Events => _events;

    public EventHandler<RobotPointUpdatedEventArgs> RobotPointUpdated { get; set; } = null!;

    public GamePoint? GetCell(int column, int row)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return null;

        return _cells[column, row];
    }

    public void Dispose()
    {
        // No unmanaged resources; disposable only to satisfy IRobotExecutor.
    }

    public event EventHandler? StateChanged;
    public event EventHandler<CellPaintedEventArgs>? CellPainted;

    public void SetMap(Map? map)
    {
        _map = map;
        Rebuild();
    }

    public void Reset()
    {
        _state = RobotState.Idle;
        _completed = false;
        _logicalCol = _spawnCol;
        _logicalRow = _spawnRow;
        _direction = RobotDirection.Down;

        lock (_stateLock)
        {
            _painted.Clear();
            _collected.Clear();
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void MoveUp() => Move(0, -1, RobotDirection.Up);
    public void MoveDown() => Move(0, 1, RobotDirection.Down);
    public void MoveLeft() => Move(-1, 0, RobotDirection.Left);
    public void MoveRight() => Move(1, 0, RobotDirection.Right);

    public void FillCell() => Paint();
    public void Draw() => Paint();

    public bool IsCellFilled()
    {
        lock (_stateLock)
            return _painted.Contains(new MapPoint(_logicalCol, _logicalRow));
    }

    public bool IsWallFromUp()
        => IsBlocked(_logicalCol, _logicalRow, 0, -1);
    
    public bool IsWallFromDown()
        => IsBlocked(_logicalCol, _logicalRow, 0, 1);
    
    public bool IsWallFromLeft()
        => IsBlocked(_logicalCol, _logicalRow, -1, 0);
    
    public bool IsWallFromRight()
        => IsBlocked(_logicalCol, _logicalRow, 1, 0);

    public bool IsPainted(int column, int row)
    {
        lock (_stateLock)
            return _painted.Contains(new MapPoint(column, row));
    }

    public bool IsCollected(int column, int row)
    {
        lock (_stateLock)
            return _collected.Contains(new MapPoint(column, row));
    }

    public void SetWall(int column, int row, WallType wallType)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return;

        GamePoint point = GetOrCreatePoint(column, row);
        point.WallType = wallType;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetCoin(int column, int row, bool value)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return;

        GamePoint point = GetOrCreatePoint(column, row);
        point.HasCoin = value;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetGoal(int column, int row, bool value)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return;

        GamePoint point = GetOrCreatePoint(column, row);
        point.IsGoal = value;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetRequireDraw(int column, int row, bool value)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return;

        GamePoint point = GetOrCreatePoint(column, row);
        point.RequireDraw = value;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSpawn(int column, int row, bool value)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return;

        GamePoint point = GetOrCreatePoint(column, row);
        if (value)
        {
            if (_map?.Points is not null)
            {
                foreach (GamePoint existing in _map.Points)
                {
                    if (existing.IsSpawn && (existing.X != column || existing.Y != row))
                        existing.IsSpawn = false;
                }
            }

            point.IsSpawn = true;
            _spawnCol = column;
            _spawnRow = row;
        }
        else
        {
            point.IsSpawn = false;
            if (_spawnCol == column && _spawnRow == row)
            {
                _spawnCol = 0;
                _spawnRow = 0;
            }
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearPainted()
    {
        lock (_stateLock)
            _painted.Clear();

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetPainted(int column, int row, bool painted)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return;

        MapPoint point = new MapPoint(column, row);
        lock (_stateLock)
        {
            if (painted)
                _painted.Add(point);
            else
                _painted.Remove(point);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearCollected()
    {
        lock (_stateLock)
            _collected.Clear();

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetCollected(int column, int row, bool collected)
    {
        if (PointOutOfBounds(column, row, _cols, _rows))
            return;

        MapPoint point = new MapPoint(column, row);
        lock (_stateLock)
        {
            if (collected)
                _collected.Add(point);
            else
                _collected.Remove(point);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private GamePoint GetOrCreatePoint(int column, int row)
    {
        GamePoint? point = _cells[column, row];
        if (point is not null)
            return point;

        point = new GamePoint { X = column, Y = row };
        _cells[column, row] = point;
        _map?.Points.Add(point);
        return point;
    }

    private void Rebuild()
    {
        _state = RobotState.Idle;
        _completed = false;
        _collected.Clear();
        _painted.Clear();

        Map? map = _map;
        if (map is null)
        {
            _cols = _rows = 0;
            _cells = new GamePoint?[0, 0];
            _logicalCol = _logicalRow = _spawnCol = _spawnRow = 0;
            StateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        _cols = Math.Max(1, map.Width);
        _rows = Math.Max(1, map.Height);
        _cells = new GamePoint?[_cols, _rows];

        GamePoint? spawn = null;
        if (map.Points is not null)
        {
            foreach (GamePoint point in map.Points)
            {
                if (PointOutOfBounds(point, _rows, _cols))
                    continue;

                _cells[point.X, point.Y] = point;
                if (point.IsSpawn)
                    spawn ??= point;
            }
        }

        _spawnCol = spawn?.X ?? 0;
        _spawnRow = spawn?.Y ?? 0;
        _logicalCol = _spawnCol;
        _logicalRow = _spawnRow;
        _direction = RobotDirection.Down;

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Move(int dCol, int dRow, RobotDirection direction)
    {
        if (_state == RobotState.Dead || _completed)
            return;

        _direction = direction;

        if (IsBlocked(_logicalCol, _logicalRow, dCol, dRow))
        {
            Die(dCol, dRow);
            return;
        }

        _logicalCol += dCol;
        _logicalRow += dRow;
        _state = RobotState.Moving;

        if (Cell(_logicalCol, _logicalRow)?.HasCoin is true)
        {
            lock (_stateLock)
                _collected.Add(new MapPoint(_logicalCol, _logicalRow));
        }

        RobotMovedEventArgs movedArgs = new RobotMovedEventArgs
        {
            NewX = _logicalCol,
            NewY = _logicalRow
        };

        Events.RobotMoved?.Invoke(this, movedArgs);
        RobotPointUpdated?.Invoke(this, new RobotPointUpdatedEventArgs
        {
            NewX = _logicalCol,
            NewY = _logicalRow
        });

        StateChanged?.Invoke(this, EventArgs.Empty);
        CheckGoals();
    }

    private void Die(int dCol, int dRow)
    {
        _state = RobotState.Dying;
        DeathDirectionColumn = dCol;
        DeathDirectionRow = dRow;

        Events.RobotDead?.Invoke(this, EventArgs.Empty);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CompleteDeath()
    {
        if (_state != RobotState.Dying)
            return;

        _state = RobotState.Dead;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Paint()
    {
        if (_state == RobotState.Dead)
            return;

        int column = _logicalCol;
        int row = _logicalRow;

        lock (_stateLock)
            _painted.Add(new MapPoint(column, row));

        CellPainted?.Invoke(this, new CellPaintedEventArgs { Column = column, Row = row });
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CheckGoals()
    {
        if (_completed || _state == RobotState.Dead)
            return;

        bool allCoins = true;
        bool hasAnyCoin = false;
        bool hasGoal = false;

        for (int rowI = 0; rowI < _rows; rowI++)
        {
            for (int colJ = 0; colJ < _cols; colJ++)
            {
                GamePoint? point = Cell(colJ, rowI);
                if (point is null)
                    continue;

                if (point.HasCoin)
                {
                    hasAnyCoin = true;
                    lock (_stateLock)
                    {
                        if (!_collected.Contains(new MapPoint(colJ, rowI)))
                            allCoins = false;
                    }
                }

                if (point.IsGoal)
                    hasGoal = true;
            }
        }

        bool onGoal = Cell(_logicalCol, _logicalRow)?.IsGoal is true;
        bool hasObjective = hasAnyCoin || hasGoal;

        if (hasObjective && allCoins && (!hasGoal || onGoal))
        {
            _completed = true;
            Events.LevelCompleted?.Invoke(this, EventArgs.Empty);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private GamePoint? Cell(int col, int row)
    {
        if (PointOutOfBounds(col, row, _cols, _rows))
            return null;

        return _cells[col, row];
    }

    private bool IsBlocked(int col, int row, int dCol, int dRow)
    {
        int targetCol = col + dCol;
        int targetRow = row + dRow;
        if (targetCol < 0 || targetRow < 0 || targetCol >= _cols || targetRow >= _rows)
            return true;

        CellWalls current = WallsOf(Cell(col, row));
        CellWalls neighbor = WallsOf(Cell(targetCol, targetRow));

        return (dCol, dRow) switch
        {
            (1, 0) => current.Right || neighbor.Left,
            (-1, 0) => current.Left || neighbor.Right,
            (0, 1) => current.Bottom || neighbor.Top,
            (0, -1) => current.Top || neighbor.Bottom,
            _ => true
        };
    }

    private static CellWalls WallEdges(WallType wall) => wall switch
    {
        WallType.None => new CellWalls(false, false, false, false),
        WallType.Full => new CellWalls(true, true, true, true),
        WallType.Top => new CellWalls(true, false, false, false),
        WallType.Bottom => new CellWalls(false, true, false, false),
        WallType.Left => new CellWalls(false, false, true, false),
        WallType.Right => new CellWalls(false, false, false, true),
        WallType.TopBottom => new CellWalls(true, true, false, false),
        WallType.TopLeft => new CellWalls(true, false, true, false),
        WallType.TopRight => new CellWalls(true, false, false, true),
        WallType.BottomLeft => new CellWalls(false, true, true, false),
        WallType.BottomRight => new CellWalls(false, true, false, true),
        WallType.LeftRight => new CellWalls(false, false, true, true),
        WallType.TopBottomLeft => new CellWalls(true, true, true, false),
        WallType.TopBottomRight => new CellWalls(true, true, false, true),
        WallType.TopLeftRight => new CellWalls(true, false, true, true),
        WallType.BottomLeftRight => new CellWalls(false, true, true, true),
        _ => new CellWalls(false, false, false, false)
    };

    private static CellWalls WallsOf(GamePoint? point)
        => point is null ? new CellWalls(false, false, false, false) : WallEdges(point.WallType);

    private static bool PointOutOfBounds(GamePoint point, int rows, int cols)
        => point.X < 0 || point.Y < 0 || point.X >= cols || point.Y >= rows;

    private static bool PointOutOfBounds(int col, int row, int cols, int rows)
        => col < 0 || row < 0 || col >= cols || row >= rows;
}
