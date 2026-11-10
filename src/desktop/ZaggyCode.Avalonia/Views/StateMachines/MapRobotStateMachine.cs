using ZaggyCode.Avalonia.Views.Records;
using ZaggyCode.Core.RobotGameCore.Enums;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Avalonia.Views.StateMachines;

public sealed class MapRobotStateMachine
{
    private RobotGameMapState _robotState = new(default);

    private RobotMap? _map;
    private GamePoint?[,] _cells = new GamePoint?[0, 0];
    private MapSize _mapSize;

    private Cell _spawnCell;
    private RobotDirection _robotDirection = RobotDirection.Down;

    public int ColumnCount => _mapSize.Columns;
    public int RowCount => _mapSize.Rows;
    public int LogicalColumn => _robotState.Position.X;
    public int LogicalRow => _robotState.Position.Y;
    public RobotDirection RobotDirection => _robotDirection;

    public event EventHandler? StateChanged;

    public GamePoint? GetCell(int column, int row)
        => GetCell(new Cell(column, row));

    public GamePoint? GetCell(Cell cell)
    {
        if (!IsWithinBounds(cell))
            return null;

        return _cells[cell.Column, cell.Row];
    }

    public GamePoint GetOrCreateCell(int column, int row)
        => GetOrCreatePoint(new Cell(column, row));

    public void SetMap(RobotMap? map)
    {
        _map = map;
        Rebuild();
    }

    public void Reset()
    {
        _robotDirection = RobotDirection.Down;
        _robotState.Reset(new System.Drawing.Point(_spawnCell.Column, _spawnCell.Row));

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetLogicalCell(int column, int row)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        _robotState.MoveTo(new System.Drawing.Point(cell.Column, cell.Row));
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetDirection(RobotDirection robotDirection)
    {
        _robotDirection = robotDirection;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDrawn(int column, int row)
        => _robotState.IsDrawn(new System.Drawing.Point(column, row));

    public bool IsCollected(int column, int row)
        => _robotState.IsCoinCollected(new System.Drawing.Point(column, row));

    public void SetWall(int column, int row, RobotWallType robotWallType)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = GetOrCreatePoint(cell);
        point.RobotWallType = robotWallType;
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

    public void ClearDrawn()
    {
        _robotState.ClearDrawn();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetDrawn(int column, int row, bool drawn)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = new System.Drawing.Point(cell.Column, cell.Row);
        if (drawn)
            _robotState.MarkDrawn(point);
        else
            _robotState.UnmarkDrawn(point);

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearCollected()
    {
        _robotState.ClearCollected();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetCollected(int column, int row, bool collected)
    {
        var cell = new Cell(column, row);
        if (!IsWithinBounds(cell))
            return;

        var point = new System.Drawing.Point(cell.Column, cell.Row);
        if (collected)
            _robotState.CollectCoin(point);
        else
            _robotState.UncollectCoin(point);

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
        var map = _map;
        if (map is null)
        {
            _mapSize = new MapSize(0, 0);
            _cells = new GamePoint?[0, 0];
            _spawnCell = new Cell(0, 0);
            _robotState = new RobotGameMapState(new System.Drawing.Point(0, 0));
            _robotDirection = RobotDirection.Down;

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
        _robotState = new RobotGameMapState(new System.Drawing.Point(_spawnCell.Column, _spawnCell.Row));
        _robotDirection = RobotDirection.Down;

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool IsWithinBounds(Cell cell) =>
        cell.Column >= 0 && cell.Row >= 0 && cell.Column < _mapSize.Columns && cell.Row < _mapSize.Rows;
}
