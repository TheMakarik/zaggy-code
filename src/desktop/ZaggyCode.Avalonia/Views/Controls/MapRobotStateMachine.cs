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
    private readonly RobotEvents _events = new();
    private readonly Lock _stateLock = new();
    private readonly HashSet<Cell> _painted = [];
    private readonly HashSet<Cell> _collected = [];

    private Map? _map;
    private GamePoint?[,] _cells = new GamePoint?[0, 0];
    private MapSize _mapSize;

    private Cell _logicalCell;
    private Cell _spawnCell;
    private CellOffset _deathDirection;
    private RobotDirection _direction = RobotDirection.Down;
    private RobotState _state = RobotState.Idle;
    private bool _completed;

    public int ColumnCount => _mapSize.Columns;
    public int RowCount => _mapSize.Rows;
    public int LogicalColumn => _logicalCell.Column;
    public int LogicalRow => _logicalCell.Row;
    public RobotDirection Direction => _direction;
    public RobotState State => _state;
    public bool IsDead => _state == RobotState.Dead;
    public bool IsCompleted => _completed;
    public int DeathDirectionColumn => _deathDirection.ColumnDelta;
    public int DeathDirectionRow => _deathDirection.RowDelta;
    public RobotEvents Events => _events;

    public EventHandler<RobotPointUpdatedEventArgs> RobotPointUpdated { get; set; } = null!;

    public event EventHandler? StateChanged;
    public event EventHandler<CellPaintedEventArgs>? CellPainted;

    public GamePoint? GetCell(int column, int row) => CellAt(new Cell(column, row));

    public void Dispose()
    {
        // No unmanaged resources; disposable only to satisfy IRobotExecutor.
    }

    public void SetMap(Map? map)
    {
        _map = map;
        Rebuild();
    }

    public void Reset()
    {
        _state = RobotState.Idle;
        _completed = false;
        _logicalCell = _spawnCell;
        _direction = RobotDirection.Down;

        lock (_stateLock)
        {
            _painted.Clear();
            _collected.Clear();
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void MoveUp() => Move(new CellOffset(0, -1), RobotDirection.Up);
    public void MoveDown() => Move(new CellOffset(0, 1), RobotDirection.Down);
    public void MoveLeft() => Move(new CellOffset(-1, 0), RobotDirection.Left);
    public void MoveRight() => Move(new CellOffset(1, 0), RobotDirection.Right);

    public void FillCell() => Paint();
    public void Draw() => Paint();

    public bool IsCellFilled()
    {
        lock (_stateLock)
            return _painted.Contains(_logicalCell);
    }

    public bool IsWallFromUp() => IsBlocked(_logicalCell, new CellOffset(0, -1));
    public bool IsWallFromDown() => IsBlocked(_logicalCell, new CellOffset(0, 1));
    public bool IsWallFromLeft() => IsBlocked(_logicalCell, new CellOffset(-1, 0));
    public bool IsWallFromRight() => IsBlocked(_logicalCell, new CellOffset(1, 0));

    public bool IsPainted(int column, int row)
    {
        lock (_stateLock)
            return _painted.Contains(new Cell(column, row));
    }

    public bool IsCollected(int column, int row)
    {
        lock (_stateLock)
            return _collected.Contains(new Cell(column, row));
    }

    public void SetWall(int column, int row, WallType wallType)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = GetOrCreatePoint(cell);
        point.WallType = wallType;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetCoin(int column, int row, bool value)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = GetOrCreatePoint(cell);
        point.HasCoin = value;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetGoal(int column, int row, bool value)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = GetOrCreatePoint(cell);
        point.IsGoal = value;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetRequireDraw(int column, int row, bool value)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = GetOrCreatePoint(cell);
        point.RequireDraw = value;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSpawn(int column, int row, bool value)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = GetOrCreatePoint(cell);
        if (value)
        {
            if (_map?.Points is not null)
            {
                foreach (var existing in _map.Points)
                {
                    if (existing.IsSpawn && (existing.X != cell.Column || existing.Y != cell.Row))
                        existing.IsSpawn = false;
                }
            }

            point.IsSpawn = true;
            _spawnCell = cell;
        }
        else
        {
            point.IsSpawn = false;
            if (_spawnCell == cell)
                _spawnCell = new Cell(0, 0);
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
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        lock (_stateLock)
        {
            if (painted)
                _painted.Add(cell);
            else
                _painted.Remove(cell);
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
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        lock (_stateLock)
        {
            if (collected)
                _collected.Add(cell);
            else
                _collected.Remove(cell);
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CompleteDeath()
    {
        if (_state != RobotState.Dying)
            return;

        _state = RobotState.Dead;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private GamePoint GetOrCreatePoint(Cell cell)
    {
        GamePoint? point = _cells[cell.Column, cell.Row];
        if (point is not null)
            return point;

        point = new GamePoint { X = cell.Column, Y = cell.Row };
        _cells[cell.Column, cell.Row] = point;
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
            _mapSize = new MapSize(0, 0);
            _cells = new GamePoint?[0, 0];
            _logicalCell = new Cell(0, 0);
            _spawnCell = new Cell(0, 0);
            StateChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        _mapSize = new MapSize(Math.Max(1, map.Width), Math.Max(1, map.Height));
        _cells = new GamePoint?[_mapSize.Columns, _mapSize.Rows];

        GamePoint? spawn = null;
        if (map.Points is not null)
        {
            foreach (GamePoint point in map.Points)
            {
                if (point.X < 0 || point.Y < 0 || point.X >= _mapSize.Columns || point.Y >= _mapSize.Rows)
                    continue;

                _cells[point.X, point.Y] = point;
                if (point.IsSpawn)
                    spawn ??= point;
            }
        }

        _spawnCell = new Cell(spawn?.X ?? 0, spawn?.Y ?? 0);
        _logicalCell = _spawnCell;
        _direction = RobotDirection.Down;

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Move(CellOffset offset, RobotDirection direction)
    {
        if (_state == RobotState.Dead || _completed)
            return;

        _direction = direction;

        if (IsBlocked(_logicalCell, offset))
        {
            Die(offset);
            return;
        }

        _logicalCell += offset;
        _state = RobotState.Moving;

        if (CellAt(_logicalCell)?.HasCoin is true)
        {
            lock (_stateLock)
                _collected.Add(_logicalCell);
        }

        var movedArgs = new RobotMovedEventArgs
        {
            NewX = _logicalCell.Column,
            NewY = _logicalCell.Row
        };

        Events.RobotMoved?.Invoke(this, movedArgs);
        RobotPointUpdated?.Invoke(this, new RobotPointUpdatedEventArgs
        {
            NewX = _logicalCell.Column,
            NewY = _logicalCell.Row
        });

        StateChanged?.Invoke(this, EventArgs.Empty);
        CheckGoals();
    }

    private void Die(CellOffset offset)
    {
        _state = RobotState.Dying;
        _deathDirection = offset;

        Events.RobotDead?.Invoke(this, EventArgs.Empty);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Paint()
    {
        if (_state == RobotState.Dead)
            return;

        lock (_stateLock)
            _painted.Add(_logicalCell);

        CellPainted?.Invoke(this, new CellPaintedEventArgs { Column = _logicalCell.Column, Row = _logicalCell.Row });
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CheckGoals()
    {
        if (_completed || _state == RobotState.Dead)
            return;

        bool allCoins = true;
        bool hasAnyCoin = false;
        bool hasGoal = false;

        for (int row = 0; row < _mapSize.Rows; row++)
        {
            for (int column = 0; column < _mapSize.Columns; column++)
            {
                GamePoint? point = CellAt(new Cell(column, row));
                if (point is null)
                    continue;

                if (point.HasCoin)
                {
                    hasAnyCoin = true;
                    lock (_stateLock)
                    {
                        if (!_collected.Contains(new Cell(column, row)))
                            allCoins = false;
                    }
                }

                if (point.IsGoal)
                    hasGoal = true;
            }
        }

        bool onGoal = CellAt(_logicalCell)?.IsGoal is true;
        bool hasObjective = hasAnyCoin || hasGoal;

        if (hasObjective && allCoins && (!hasGoal || onGoal))
        {
            _completed = true;
            Events.LevelCompleted?.Invoke(this, EventArgs.Empty);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private GamePoint? CellAt(Cell cell)
    {
        if (!IsWithinBounds(cell))
            return null;

        return _cells[cell.Column, cell.Row];
    }

    private bool IsBlocked(Cell cell, CellOffset offset)
    {
        Cell target = cell + offset;
        if (!IsWithinBounds(target))
            return true;

        CellWalls current = WallsOf(CellAt(cell));
        CellWalls neighbor = WallsOf(CellAt(target));

        return (offset.ColumnDelta, offset.RowDelta) switch
        {
            (1, 0) => current.Right || neighbor.Left,
            (-1, 0) => current.Left || neighbor.Right,
            (0, 1) => current.Bottom || neighbor.Top,
            (0, -1) => current.Top || neighbor.Bottom,
            _ => true
        };
    }

    private bool IsWithinBounds(Cell cell) =>
        cell.Column >= 0 && cell.Row >= 0 && cell.Column < _mapSize.Columns && cell.Row < _mapSize.Rows;

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
}
