using ZaggyCode.Avalonia.Views.Records;

namespace ZaggyCode.Avalonia.Views.StateMachines;

public sealed class MapRobotStateMachine
{
    private readonly Lock _stateLock = new();
    private readonly HashSet<Cell> _painted = [];
    private readonly HashSet<Cell> _collected = [];

    private Map? _map;
    private GamePoint?[,] _cells = new GamePoint?[0, 0];
    private MapSize _mapSize;

    private Cell _logicalCell;
    private Cell _spawnCell;
    private Direction _direction = Direction.Down;

    public int ColumnCount => _mapSize.Columns;
    public int RowCount => _mapSize.Rows;
    public int LogicalColumn => _logicalCell.Column;
    public int LogicalRow => _logicalCell.Row;
    public Direction Direction => _direction;

    public event EventHandler? StateChanged;

    public GamePoint? GetCell(int column, int row)
        => GetCell(new Cell(column, row));

    public GamePoint? GetCell(Cell cell)
    {
        if (!IsWithinBounds(cell))
            return null;

        return _cells[cell.Column, cell.Row];
    }

    public void SetMap(Map? map)
    {
        _map = map;
        Rebuild();
    }

    public void Reset()
    {
        _logicalCell = _spawnCell;
        _direction = Direction.Down;

        lock (_stateLock)
        {
            _painted.Clear();
            _collected.Clear();
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetLogicalCell(int column, int row)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        _logicalCell = cell;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetDirection(Direction direction)
    {
        _direction = direction;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

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

    private GamePoint GetOrCreatePoint(Cell cell)
    {
        var point = _cells[cell.Column, cell.Row];
        if (point is not null)
            return point;

        point = new GamePoint { X = cell.Column, Y = cell.Row };
        _cells[cell.Column, cell.Row] = point;
        _map?.Points.Add(point);
        return point;
    }

    private void Rebuild()
    {
        _collected.Clear();
        _painted.Clear();

        var map = _map;
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
            foreach (var point in map.Points)
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
        _direction = Direction.Down;

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool IsWithinBounds(Cell cell) =>
        cell.Column >= 0 && cell.Row >= 0 && cell.Column < _mapSize.Columns && cell.Row < _mapSize.Rows;
}
