namespace ZaggyCode.Avalonia.Views.Controls;

// Game field rendered as an island: cells are grass, walls/obstacles are rocks,
// the surrounding border is sea. Exposes an IRobotExecutor (Executor) whose
// movement/draw methods drive the robot animation on this control.
public sealed class MapView : Control
{
    private record struct MapPoint(int X, int Y);
    private record struct CellWalls(bool Top, bool Bottom, bool Left, bool Right);

    private const double TickMilliseconds = 16.0;

    private static readonly IBrush SeaBrush = new SolidColorBrush(Color.FromRgb(0x2C, 0x5F, 0x8A));
    private static readonly IBrush GrassBrush = new SolidColorBrush(Color.FromRgb(0x5F, 0xA8, 0x5A));
    private static readonly IBrush GrassDarkBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x9A, 0x52));
    private static readonly IBrush RockBrush = new SolidColorBrush(Color.FromRgb(0x8A, 0x8F, 0x96));
    private static readonly IBrush PaintedBrush = new SolidColorBrush(Color.FromArgb(115, 0xE0, 0xA9, 0x3B));
    private static readonly IBrush RobotBrush = new SolidColorBrush(Color.FromRgb(0xF2, 0xC1, 0x4E));
    private static readonly IBrush RobotDeadBrush = new SolidColorBrush(Color.FromRgb(0xD8, 0x70, 0x60));
    private static readonly IBrush EyeBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));
    private static readonly IBrush CoinBrush = new SolidColorBrush(Color.FromRgb(0xF2, 0xC1, 0x4E));

    private static readonly IPen GrassLinePen = new Pen(new SolidColorBrush(Color.FromRgb(0x4E, 0x8C, 0x4A)), 1);
    private static readonly IPen RockEdgePen = new Pen(new SolidColorBrush(Color.FromRgb(0x6F, 0x74, 0x7C)), 1);
    private static readonly IPen EyePen = new Pen(EyeBrush, 1.5);
    private static readonly IPen GoalPen = new Pen(new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), 2);

    private Map? _map;
    private GamePoint?[,] _cells = new GamePoint?[0, 0];
    private int _cols;
    private int _rows;
    private int _logicalCol;
    private int _logicalRow;
    private double _renderCol;
    private double _renderRow;

    private volatile bool _completed;
    private volatile bool _isDead;
    private bool _deadPose;
    private int _spawnCol, _spawnRow;

    private readonly Lock _stateLock = new();
    private readonly HashSet<MapPoint> _painted = [];
    private readonly HashSet<MapPoint> _collected = [];

    private DispatcherTimer? _timer;

    private double _moveFromCol;
    private double _moveFromRow;
    private double _moveToCol;
    private double _moveToRow;
    private double _moveElapsed;
    private double _moveDuration;
    private bool _moveActive;

    private int _deathCellCol;
    private int _deathCellRow;
    private int _deathDx;
    private int _deathDy;
    private double _deathElapsed;
    private double _deathDuration;
    private bool _deathActive;

    private int _flashCol;
    private int _flashRow;
    private double _flashElapsed;
    private double _flashDuration;
    private bool _flashActive;

    public Map? Map
    {
        get => _map;
        set => SetAndRaise(MapProperty, ref _map, value);
    }

    public TimeSpan StepDuration
    {
        get => GetValue(StepDurationProperty);
        set => SetValue(StepDurationProperty, value);
    }

    public RobotEvents Events => field ??= new RobotEvents();
    public IRobotExecutor Executor => field ??= new MapRobotExecutor(this);

    public bool IsDead => _isDead;
    public bool IsCompleted => _completed;

    public static readonly StyledProperty<TimeSpan> StepDurationProperty =
        AvaloniaProperty.Register<MapView, TimeSpan>(nameof(StepDuration), TimeSpan.FromMilliseconds(220));

    public static readonly DirectProperty<MapView, Map?> MapProperty =
        AvaloniaProperty.RegisterDirect<MapView, Map?>(nameof(Map), o => o.Map, (o, v) => o.Map = v);

    public MapView()
    {
        Focusable = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MapProperty)
            OnMapChanged(change.OldValue as Map, change.NewValue as Map);
    }

    private void OnMapChanged(Map? oldMap, Map? newMap)
    {
        oldMap?.CollectionChanged -= OnMapPointsChanged;
        newMap?.CollectionChanged += OnMapPointsChanged;

        Dispatcher.UIThread.Post(Rebuild);
    }

    private void OnMapPointsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(Rebuild);
    }

    // Small demonstration island; replaced once the level loader exists.
    public static Map CreateSampleMap()
    {
        const int width = 8;
        const int height = 6;

        GamePoint[,] grid = new GamePoint[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                grid[x, y] = new GamePoint { X = x, Y = y };
        }

        // A rock wall running between columns 3 and 4 for the middle rows.
        for (int y = 1; y <= 3; y++)
        {
            grid[3, y].WallType = WallType.Right;
            grid[4, y].WallType = WallType.Left;
        }

        // A solid boulder the robot can neither enter nor pass.
        grid[6, 1].WallType = WallType.Full;
        grid[1, 4].IsSpawn = true;

        // Collect every coin and reach the goal in the bottom-right corner.
        grid[1, 1].HasCoin = true;
        grid[2, 2].HasCoin = true;
        grid[5, 3].HasCoin = true;
        grid[7, 5].IsGoal = true;

        ObservableCollection<GamePoint> points = [];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                points.Add(grid[x, y]);
        }

        return new Map
        {
            Width = width,
            Height = height,
            Points = points
        };
    }
    
    public void Reset()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(Reset);
            return;
        }

        StopTimer();
        _moveActive = _deathActive = _flashActive = false;
        _isDead = false;
        _deadPose = false;
        _completed = false;

        _logicalCol = _spawnCol;
        _logicalRow = _spawnRow;
        _renderCol = _spawnCol;
        _renderRow = _spawnRow;

        lock (_stateLock)
        {
            _painted.Clear();
            _collected.Clear();
        }

        InvalidateVisual();
    }

    // Победа = собрать все монетки и наступить на финиш
    private void CheckGoals()
    {
        if (_completed || _isDead)
            return;

        bool allCoins = true;
        bool hasAnyCoin = false;
        bool hasGoal = false;

        for (int rowI = 0; rowI < _rows; rowI++)
        {
            for (int colJ = 0; colJ < _cols; colJ++)
            {
                GamePoint? point = _cells[colJ, rowI];
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
            Dispatcher.UIThread.Post(InvalidateVisual);

            EventHandler handler = Events.LevelCompleted;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Rebuild()
    {
        StopTimer();
        _moveActive = _deathActive = _flashActive = false;
        _isDead = _deadPose = false;

        Map? map = _map;
        if (map is null)
        {
            _cols = _rows = 0;
            _cells = new GamePoint?[0, 0];

            InvalidateVisual();
            InvalidateMeasure();
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

        int spawnX = spawn?.X ?? 0;
        int spawnY = spawn?.Y ?? 0;

        _spawnCol = spawnX;
        _spawnRow = spawnY;
        _logicalCol = spawnX;
        _logicalRow = spawnY;
        _renderCol = spawnX;
        _renderRow = spawnY;

        lock (_stateLock)
            _painted.Clear();

        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled)
            return;

        switch (e.Key)
        {
            case Key.Up:
                {
                    Executor.MoveUp();
                    e.Handled = true;
                    break;
                }

            case Key.Down:
                {
                    Executor.MoveDown();
                    e.Handled = true;
                    break;
                }

            case Key.Left:
                {
                    Executor.MoveLeft();
                    e.Handled = true;
                    break;
                }

            case Key.Right:
                {
                    Executor.MoveRight();
                    e.Handled = true;
                    break;
                }

            case Key.D:
                {
                    Executor.FillCell();
                    e.Handled = true;
                    break;
                }

            case Key.R:
                {
                    Reset();
                    e.Handled = true;
                    break;
                }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_cols == 0 || _rows == 0)
            return default;

        const double cell = 40;
        double width = _cols * cell;
        double height = _rows * cell;

        if (double.IsFinite(availableSize.Width) && availableSize.Width > 0)
            width = Math.Min(width, availableSize.Width);

        if (double.IsFinite(availableSize.Height) && availableSize.Height > 0)
            height = Math.Min(height, availableSize.Height);

        return new Size(width, height);
    }

    public override void Render(DrawingContext context)
    {
        Size size = Bounds.Size;
        context.DrawRectangle(SeaBrush, null, new Rect(size));

        if (_cols == 0 || _rows == 0)
            return;

        double cell = Math.Clamp(Math.Min(size.Width / _cols, size.Height / _rows), 6, 80);
        double islandW = cell * _cols;
        double islandH = cell * _rows;

        double offsetX = (size.Width - islandW) / 2.0;
        double offsetY = (size.Height - islandH) / 2.0;

        double rockThickness = Math.Clamp(cell * 0.16, 2, 14);

        for (int rowI = 0; rowI < _rows; rowI++)
        {
            for (int columnJ = 0; columnJ < _cols; columnJ++)
            {
                double rowX = offsetX + columnJ * cell;
                double rowY = offsetY + rowI * cell;

                Rect rect = new Rect(rowX, rowY, cell, cell);
                GamePoint? point = Cell(columnJ, rowI);
                CellWalls walls = WallsOf(point);

                bool isOdd = ((columnJ + rowI) & 1) == 1;
                context.DrawRectangle(isOdd ? GrassDarkBrush : GrassBrush, null, rect);

                bool painted;
                lock (_stateLock)
                    painted = _painted.Contains(new MapPoint(columnJ, rowI));

                if (painted || (point?.RequireDraw ?? false))
                    context.DrawRectangle(PaintedBrush, null, rect);

                if (point is not null && point.IsGoal)
                {
                    context.DrawEllipse(null, GoalPen, new Rect(
                        rowX + cell * 0.26,
                        rowY + cell * 0.26,
                        cell * 0.48,
                        cell * 0.48));
                }

                bool collected;
                lock (_stateLock)
                    collected = _collected.Contains(new MapPoint(columnJ, rowI));

                if (point is not null && point.HasCoin && !collected)
                {
                    context.DrawEllipse(CoinBrush, null, new Rect(
                        rowX + cell * 0.36,
                        rowY + cell * 0.36,
                        cell * 0.28,
                        cell * 0.28));
                }

                if (point is not null && point.WallType == WallType.Full)
                {
                    context.DrawRectangle(RockBrush, RockEdgePen, rect);
                    continue;
                }

                if (walls.Top && rowI > 0)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY,
                        cell,
                        rockThickness));
                }

                if (walls.Bottom && rowI < _rows - 1)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY + cell - rockThickness,
                        cell,
                        rockThickness));
                }

                if (walls.Left && columnJ > 0)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX,
                        rowY,
                        rockThickness,
                        cell));
                }

                if (walls.Right && columnJ < _cols - 1)
                {
                    context.DrawRectangle(RockBrush, null, new Rect(
                        rowX + cell - rockThickness,
                        rowY,
                        rockThickness,
                        cell));
                }
            }
        }

        context.DrawRectangle(null, GrassLinePen, new Rect(offsetX, offsetY, islandW, islandH));

        if (_flashActive)
        {
            double t = Math.Clamp(_flashElapsed / _flashDuration, 0, 1);
            byte alpha = (byte)(200 * (1 - t));

            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(alpha, 0xFF, 0xFF, 0xFF)), null, new Rect(
                offsetX + _flashCol * cell,
                offsetY + _flashRow * cell,
                cell,
                cell));
        }

        DrawRobot(context, offsetX, offsetY, cell);
    }

    private void DrawRobot(DrawingContext context, double offsetX, double offsetY, double cell)
    {
        double centerX = offsetX + (_renderCol + 0.5) * cell;
        double centerY = offsetY + (_renderRow + 0.5) * cell;
        double body = cell * 0.62;

        context.DrawRectangle(_deadPose ? RobotDeadBrush : RobotBrush, null, new Rect(
            centerX - body / 2,
            centerY - body / 2,
            body,
            body));

        double eyeRadius = body * 0.10;
        double eyeOffsetX = body * 0.20;
        double eyeOffsetY = -body * 0.06;

        if (_deadPose)
        {
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Point eye = new Point(centerX + sign * eyeOffsetX, centerY + eyeOffsetY);
                context.DrawLine(EyePen,
                    new Point(eye.X - eyeRadius, eye.Y - eyeRadius),
                    new Point(eye.X + eyeRadius, eye.Y + eyeRadius));

                context.DrawLine(EyePen,
                    new Point(eye.X - eyeRadius, eye.Y + eyeRadius),
                    new Point(eye.X + eyeRadius, eye.Y - eyeRadius));
            }
        }
        else
        {
            double eyeSize = eyeRadius * 2;
            context.DrawEllipse(EyeBrush, null, new Rect(
                centerX - eyeOffsetX - eyeRadius,
                centerY + eyeOffsetY - eyeRadius,
                eyeSize,
                eyeSize));

            context.DrawEllipse(EyeBrush, null, new Rect(
                centerX + eyeOffsetX - eyeRadius,
                centerY + eyeOffsetY - eyeRadius,
                eyeSize,
                eyeSize));
        }
    }

    private GamePoint? Cell(int col, int row)
    {
        if (PointOutOfBounds(col, row, _cols, _rows))
            return null;

        return _cells[col, row];
    }

    // True if the robot at (col,row) cannot step by (dCol,dRow): off the island (sea) or through a wall edge.
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

    private void Move(int dCol, int dRow)
    {
        if (_isDead || _completed)
            return;

        if (IsBlocked(_logicalCol, _logicalRow, dCol, dRow))
        {
            Die(_logicalCol, _logicalRow);
            return;
        }

        _logicalCol += dCol;
        _logicalRow += dRow;

        if (Cell(_logicalCol, _logicalRow)?.HasCoin is true)
        {
            lock (_stateLock)
                _collected.Add(new MapPoint(_logicalCol, _logicalRow));
        }

        int targetCol = _logicalCol;
        int targetRow = _logicalRow;

        Events.RobotMoved?.Invoke(this, new RobotMovedEventArgs
        {
            NewX = targetCol,
            NewY = targetRow
        });

        Dispatcher.UIThread.Post(() => StartMove(targetCol, targetRow));
        CheckGoals();
    }

    private void Die(int dCol, int dRow)
    {
        _isDead = true;

        Events.RobotDead?.Invoke(this, EventArgs.Empty);

        int cellCol = _logicalCol;
        int cellRow = _logicalRow;
        Dispatcher.UIThread.Post(() => StartDeath(cellCol, cellRow, dCol, dRow));
    }

    private void Paint()
    {
        if (_isDead)
            return;

        int col = _logicalCol;
        int row = _logicalRow;

        lock (_stateLock)
            _painted.Add(new MapPoint(col, row));

        Dispatcher.UIThread.Post(() => StartFlash(col, row));
    }

    private void StartMove(int toCol, int toRow)
    {
        _moveFromCol = _renderCol;
        _moveFromRow = _renderRow;

        _moveToCol = toCol;
        _moveToRow = toRow;

        _moveElapsed = 0;
        _moveDuration = StepDuration.TotalMilliseconds;

        _moveActive = true;
        EnsureTimer();
    }

    private void StartDeath(int cellCol, int cellRow, int dCol, int dRow)
    {
        // A valid move may still be mid-tween; cancel it so the bonk lunge starts cleanly from the cell.
        _moveActive = false;
        _renderCol = cellCol;
        _renderRow = cellRow;

        _deathCellCol = cellCol;
        _deathCellRow = cellRow;

        _deathDx = dCol;
        _deathDy = dRow;

        _deathElapsed = 0;
        _deathDuration = StepDuration.TotalMilliseconds * 1.3;

        _deathActive = true;
        _deadPose = false;

        EnsureTimer();
    }

    private void StartFlash(int col, int row)
    {
        _flashCol = col;
        _flashRow = row;

        _flashElapsed = 0;
        _flashDuration = 200;

        _flashActive = true;
        EnsureTimer();
    }

    private void EnsureTimer()
    {
        if (_timer is null)
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TickMilliseconds)
            };

            _timer.Tick += OnTimerTick;
        }

        if (!_timer.IsEnabled)
            _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer is not null && _timer.IsEnabled)
            _timer.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        bool any = false;

        if (_moveActive)
        {
            _moveElapsed += TickMilliseconds;

            double t = Math.Clamp(_moveElapsed / _moveDuration, 0, 1);
            double eased = EaseOut(t);

            _renderCol = _moveFromCol + (_moveToCol - _moveFromCol) * eased;
            _renderRow = _moveFromRow + (_moveToRow - _moveFromRow) * eased;

            if (t >= 1)
            {
                _renderCol = _moveToCol;
                _renderRow = _moveToRow;
                _moveActive = false;
            }

            any = true;
        }

        if (_deathActive)
        {
            _deathElapsed += TickMilliseconds;

            double t = Math.Clamp(_deathElapsed / _deathDuration, 0, 1);
            double offset = DeathOffset(t);

            _renderCol = _deathCellCol + _deathDx * offset;
            _renderRow = _deathCellRow + _deathDy * offset;

            if (t >= 1)
            {
                _renderCol = _deathCellCol;
                _renderRow = _deathCellRow;
                _deathActive = false;
                _deadPose = true;
            }

            any = true;
        }

        if (_flashActive)
        {
            _flashElapsed += TickMilliseconds;
            if (_flashElapsed >= _flashDuration)
                _flashActive = false;

            any = true;
        }

        InvalidateVisual();

        if (!any)
            StopTimer();
    }

    // Lunges toward the blocked direction then recoils into the cell.
    private static double DeathOffset(double t)
    {
        const double amplitude = 0.32;
        if (t < 0.4)
        {
            double phase = t / 0.4;
            return amplitude * EaseOut(phase);
        }

        double recoil = (t - 0.4) / 0.6;
        return amplitude * (1 - recoil * recoil * recoil);
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

    private bool PointOutOfBounds(GamePoint point, int rows, int cols)
        => point.X < 0 || point.Y < 0 || point.X >= cols || point.Y >= rows;

    private bool PointOutOfBounds(int row, int col, int rows, int cols)
        => col < 0 || row < 0 || col >= cols || row >= rows;

    private bool CanMove(int dCol, int dRow)
        => !IsBlocked(_logicalCol, _logicalRow, dCol, dRow);

    private static double EaseOut(double t)
        => 1 - (1 - t) * (1 - t) * (1 - t);

    private static CellWalls WallsOf(GamePoint? point)
        => point is null ? new CellWalls(false, false, false, false) : WallEdges(point.WallType);

    private sealed class MapRobotExecutor(MapView owner) : IRobotExecutor
    {
        private readonly MapView _owner = owner;

        public void MoveUp() => _owner.Move(0, -1);
        public void MoveDown() => _owner.Move(0, 1);
        public void MoveLeft() => _owner.Move(-1, 0);
        public void FillCell()
        {
            _owner.Paint();
        }

        public bool IsCellFilled()
        {
            throw new NotImplementedException();
        }

        public void MoveRight() => _owner.Move(1, 0);
        public void Draw() => _owner.Paint();

        public bool IsWallFromUp() => _owner.IsBlocked(_owner._logicalCol, _owner._logicalRow, 0, -1);
        public bool IsWallFromDown() => _owner.IsBlocked(_owner._logicalCol, _owner._logicalRow, 0, 1);
        public bool IsWallFromLeft() => _owner.IsBlocked(_owner._logicalCol, _owner._logicalRow, -1, 0);
        public bool IsWallFromRight() => _owner.IsBlocked(_owner._logicalCol, _owner._logicalRow, 1, 0);
    }
}
